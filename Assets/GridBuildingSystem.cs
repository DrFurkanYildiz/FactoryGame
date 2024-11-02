using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }

    private Grid<Tile> _grid;
    private PlacedObjectTypeSo _placedObjectTypeSo;
    [SerializeField] private List<PlacedObjectTypeSo> placedObjectTypeSOList = null;
    [SerializeField] private Dir _dir;
    private Transform _visual;

    public bool isDemolishActive;

    private bool _isFirstConveyorTunnelPlaced;
    private PlacedObject _firstPlacedTunnel;
    private const int MaxTunnelLength = 5;


    private PathfindingSystem<Tile> _pathfindingSystem;
    private bool _isConveyorBeltSelected;
    private Vector2Int _firstConveyorBeltCoordinate;
    private Vector2Int _lastConveyorBeltCoordinate;
    private bool _isFirstConveyorBeltCoordinateSelected;
    
    private bool _isFirstBeltPlaced;
    private PlacedObject _firstPlacedBelt;
    private List<Transform> _beltVisualList = new List<Transform>();
    [SerializeField] private List<Vector3> _beltPathPositionList = new List<Vector3>();
    private Vector2Int _lastMouseGridPosition;

    public Action<PlacedObjectType> OnSelectedPlacedObject;

    private void Awake()
    {
        Instance = this;
        _grid = InitializeGrid(20, 20, 1f);
        _pathfindingSystem = new PathfindingSystem<Tile>(_grid);

        OnSelectedPlacedObject += SelectObject;
    }
    private static Grid<Tile> InitializeGrid(int width, int height, float cellSize)
    {
        return new Grid<Tile>(width, height, cellSize, Vector3.zero, 
            (Grid<Tile> g, int x, int y) => new Tile(g, x, y));
    }

    private void Start()
    {
        
        var conveyorSplitter = PlacedObject.Create(_grid, Vector3.zero, Vector2Int.zero, _dir,
            GetPlacedSo(PlacedObjectType.ConveyorSplitter));
    }

    private void Update()
    {
        HandleSelectObject();
        HandleRotateObject();
        HandleDemolish();
            
        if (_isFirstConveyorBeltCoordinateSelected)
        {
            var mousePosition = Mouse3D.GetMouseWorldPosition();
            _grid.GetXZ(mousePosition, out var x, out var z);
            var currentMouseGridPosition = new Vector2Int(x, z);

            // Sadece fare yeni bir hücreye geçtiğinde çalıştır
            if (currentMouseGridPosition != _lastMouseGridPosition && currentMouseGridPosition != _firstConveyorBeltCoordinate)
            {
                _lastMouseGridPosition = currentMouseGridPosition;

                // Görselleri temizle
                foreach (var beltVisual in _beltVisualList)
                {
                    Destroy(beltVisual.gameObject);
                }

                _beltVisualList.Clear();

                // Yeni yol hesapla
                _beltPathPositionList = FindBeltPath(_firstConveyorBeltCoordinate, currentMouseGridPosition);
/*
                //Sadece 1 tane belt yapılacak
                if (_beltPathPositionList.Count <= 0)
                {
                    // Belt görselini oluştur
                    var beltVisual = Instantiate(
                        GetPlacedSo(PlacedObjectType.ConveyorBelt).visual, _grid.GetWorldPosition(_firstConveyorBeltCoordinate),Quaternion.identity);
                    beltVisual.parent = transform;
                    _beltVisualList.Add(beltVisual);
                    return;
                }
                */
                
                // Yeni yol için görseller oluştur
                if (_beltPathPositionList.Count > 0)
                {
                    
                    var lastIndex = _beltPathPositionList.Count - 1;
                    var lastPos = _beltPathPositionList[lastIndex];
                    _beltPathPositionList.RemoveAt(lastIndex);
                    _beltPathPositionList.Insert(0, _grid.GetWorldPosition(_firstConveyorBeltCoordinate));
                    _beltPathPositionList.Add(lastPos);
                    
                    for (int i = 0; i < _beltPathPositionList.Count; i++)
                    {
                        //Vector3 currentPos = _grid.GetWorldPosition(_beltPathPositionList[i]);
                        Vector3 currentPos = Vector3.zero;
                        Quaternion rotation = Quaternion.identity;
/*
                        // Eğer bir sonraki pozisyon varsa, yönü belirlemek için kullan
                        if (i < _beltPathPositionList.Count - 1)
                        {
                            Vector3 nextPos = _grid.GetWorldPosition(_beltPathPositionList[i + 1]);

                            
                            var dir = PlacedObjectTypeSo.GetDir(_grid.GetCoordinate(currentPos),
                                _grid.GetCoordinate(nextPos));

                            //TODO: Rot Pos tamamne bozuldu.
                            currentPos = GetMouseWorldSnappedPosition(GetPlacedSo(PlacedObjectType.ConveyorBelt),
                                _beltPathPositionList[i], _dir);
                            
                            rotation = GetPlacedObjectRotation(dir);
                        }
                        */
                        currentPos = GetMouseWorldSnappedPosition(GetPlacedSo(PlacedObjectType.ConveyorBelt),
                            _beltPathPositionList[i], _dir);
                            
                        rotation = GetPlacedObjectRotation(_dir);

                        // Belt görselini oluştur
                        var beltVisual = Instantiate(
                            GetPlacedSo(PlacedObjectType.ConveyorBelt).visual, transform);
                        beltVisual.position = currentPos;
                        beltVisual.rotation = rotation;
                        _beltVisualList.Add(beltVisual);
                    }
                }
            }
        }


        if (_isFirstConveyorTunnelPlaced)
        {
            var mousePosition = Mouse3D.GetMouseWorldPosition();
            _grid.GetXZ(mousePosition, out var x, out var z);
            var placedObjectOrigin = new Vector2Int(x, z);
            if (IsSecondTunnelValid(placedObjectOrigin))
                HandlePlacement();
        }
        else
        {
            HandlePlacement();
        }
    }

    private void LateUpdate()
    {
        MoveVisual();
    }

    private void MoveVisual()
    {
        if (_visual == null) return;
        var targetPosition = GetMouseWorldSnappedPosition();
        targetPosition.y = 0f;
        _visual.position = Vector3.Lerp(_visual.position, targetPosition, Time.deltaTime * 15f);
        //_visual.position = targetPosition;
        _visual.rotation = Quaternion.Lerp(_visual.rotation, GetPlacedObjectRotation(), Time.deltaTime * 15f);
    }

    private void RefreshVisual()
    {
        if (_visual != null) Destroy(_visual.gameObject);
        if (_placedObjectTypeSo == null || _isFirstBeltPlaced) return;
        _visual = Instantiate(_placedObjectTypeSo.visual, transform);
        _visual.SetLocalPositionAndRotation(GetMouseWorldSnappedPosition(), Quaternion.identity);
    }

    private void HandleSelectObject()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) OnSelectedPlacedObject?.Invoke(PlacedObjectType.Storage);
        if (Input.GetKeyDown(KeyCode.Alpha2)) OnSelectedPlacedObject?.Invoke(PlacedObjectType.ConveyorBelt);
        if (Input.GetKeyDown(KeyCode.Alpha3)) OnSelectedPlacedObject?.Invoke(PlacedObjectType.ConveyorSplitter);
        if (Input.GetKeyDown(KeyCode.Alpha4)) OnSelectedPlacedObject?.Invoke(PlacedObjectType.Machine1);
        if (Input.GetKeyDown(KeyCode.Alpha5)) OnSelectedPlacedObject?.Invoke(PlacedObjectType.ConveyorTunnel);
        if (Input.GetKeyDown(KeyCode.Alpha6)) OnSelectedPlacedObject?.Invoke(PlacedObjectType.Test);
        
        
        //TODO: Deselect ve Demolish refaktör edilecek.
        if (!isDemolishActive && Input.GetMouseButtonDown(1))
        {
            DeselectObjectType();
            if (_isFirstConveyorTunnelPlaced)
            {
                _isFirstConveyorTunnelPlaced = false;
                _firstPlacedTunnel.DestroySelf();
            }

            if (_isFirstBeltPlaced)
            {
                _firstPlacedBelt.DestroySelf();
                ConveyorBeltsPathClear();
            }
        }
    }

    
    private void SelectObject(PlacedObjectType placedObjectType)
    {
        _placedObjectTypeSo = GetPlacedSo(placedObjectType);
        _isConveyorBeltSelected = placedObjectType == PlacedObjectType.ConveyorBelt;
        RefreshVisual();
    }
    
    private PlacedObjectTypeSo GetPlacedSo(PlacedObjectType type)
    {
        return placedObjectTypeSOList.Find(t => t.type == type);
    }
    
    private void HandleRotateObject()
    {
        if (Input.GetKeyDown(KeyCode.R))
            _dir = PlacedObjectTypeSo.GetNextDir(_dir);
    }

    private void HandlePlacement()
    {
        if (Input.GetMouseButton(0))
        {
            if (_placedObjectTypeSo != null)
            {
                var mousePosition = Mouse3D.GetMouseWorldPosition();
                ObjectPlacement(_grid, _placedObjectTypeSo, mousePosition, _dir, out var placedObject);
                /*
                 if (_isConveyorBeltSelected)
                 {
                     _firstConveyorBeltCoordinate = _grid.GetCoordinate(mousePosition);
                     _isFirstConveyorBeltCoordinateSelected = true;


                     // Belt görselini oluştur
                     var beltVisual = Instantiate(
                         GetPlacedSo(PlacedObjectType.ConveyorBelt).visual, transform);
                     beltVisual.position = GetMouseWorldSnappedPosition();
                     beltVisual.rotation = GetPlacedObjectRotation();
                     Debug.Log("ilk belt görseli oluşturuldu! "+_grid.GetWorldPosition(_firstConveyorBeltCoordinate));
                     _beltVisualList.Add(beltVisual);

                     DeselectObjectType();
                     return;
                 }

                 ObjectPlacement(_grid, _placedObjectTypeSo, mousePosition, _dir, out var placedObject);

                 if (placedObject == null) return;

                 if (placedObject is ConveyorTunnel)
                 {
                     ConveyorTunnelAssignment(placedObject);
                     return;
                 }

                 if (placedObject is not BeltTest)
                     DeselectObjectType();
                     */
            }
            
            /*
            else
            {
                if (!_isFirstBeltPlaced) return;
                
                foreach (var pos in _beltPathPositionList)
                    ObjectPlacement(_grid, GetPlacedSo(PlacedObjectType.ConveyorBelt), pos, _dir, out var placedObject);

                ConveyorBeltsPathClear();
                DeselectObjectType();
            }
            
            */
        }
    }

    private void HandleDemolish()
    {
        if (isDemolishActive && Input.GetMouseButtonDown(1))
        {
            var placedObject = _grid.GetGridObject(Mouse3D.GetMouseWorldPosition()).OwnedObject;
            if (placedObject == null) return;

            placedObject.DestroySelf();
            var gridPositionList = placedObject.GetGridPositionList();
            foreach (var gridPosition in gridPositionList)
                _grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
        }
    }

    private void ObjectPlacement(Grid<Tile> grid, PlacedObjectTypeSo placedObjectTypeSo, Vector3 selectPosition,
        Dir dir, out PlacedObject placedObject)
    {
        grid.GetXZ(selectPosition, out var x, out var z);
        var placedObjectOrigin = new Vector2Int(x, z);

        var gridPositionList = placedObjectTypeSo.GetGridPositionList(placedObjectOrigin, dir);
        var canBuild =
            gridPositionList.All(gridPosition => grid.GetGridObject(gridPosition.x, gridPosition.y).canBuild);

        if (!canBuild)
        {
            placedObject = null;
            return;
        }

        var rotationOffset = placedObjectTypeSo.GetRotationOffset(dir);
        var placedObjectWorldPosition = grid.GetWorldPosition(x, z) +
                                        new Vector3(rotationOffset.x, 0, rotationOffset.y) *
                                        grid.GetCellSize();
        placedObject =
            PlacedObject.Create(grid, placedObjectWorldPosition, placedObjectOrigin, dir, placedObjectTypeSo);

        foreach (var gridPosition in gridPositionList)
            grid.GetGridObject(gridPosition.x, gridPosition.y).OwnedObject = placedObject;
    }


    private void DeselectObjectType()
    {
        _placedObjectTypeSo = null;
        RefreshVisual();
    }


    private Vector3 GetMouseWorldSnappedPosition()
    {
        if (_placedObjectTypeSo == null) return Mouse3D.GetMouseWorldPosition();
        Vector2Int rotationOffset = _placedObjectTypeSo.GetRotationOffset(_dir);
        Vector3 placedObjectWorldPosition = _grid.GetWorldPosition(Mouse3D.GetMouseWorldPosition()) +
                                            new Vector3(rotationOffset.x, 0, rotationOffset.y) * _grid.GetCellSize();
        return placedObjectWorldPosition;
    }

    private Vector3 GetMouseWorldSnappedPosition(PlacedObjectTypeSo placedObjectTypeSo, Vector3 position, Dir dir)
    {
        var rotationOffset = placedObjectTypeSo.GetRotationOffset(dir);
        return _grid.GetWorldPosition(position) +
               new Vector3(rotationOffset.x, 0, rotationOffset.y) * _grid.GetCellSize();
    }

    private Quaternion GetPlacedObjectRotation()
    {
        return _placedObjectTypeSo != null
            ? Quaternion.Euler(0, PlacedObjectTypeSo.GetRotationAngle(_dir), 0)
            : Quaternion.identity;
    }
    private Quaternion GetPlacedObjectRotation(Dir dir)
    {
        return Quaternion.Euler(0, PlacedObjectTypeSo.GetRotationAngle(dir), 0);
    }

    private List<Vector3> FindBeltPath(Vector2Int start, Vector2Int end)
    {
        var path = _pathfindingSystem.FindPath(_grid.GetWorldPosition(start), _grid.GetWorldPosition(end), false);
        if (path == null) Debug.Log("Path not found!");//Debug.DrawLine(path[i], path[i + 1], Color.red, 1f);
        return path;
    }
    
    private void ConveyorBeltsPathClear()
    {
        _isFirstBeltPlaced = false;
        _firstPlacedBelt = null;

        foreach (var beltVisual in _beltVisualList)
            Destroy(beltVisual.gameObject);
        
        _beltVisualList.Clear();
        _beltPathPositionList.Clear();
    }
    private void ConveyorTunnelAssignment(PlacedObject placedObject)
    {
        if (!_isFirstConveyorTunnelPlaced)
        {
            _isFirstConveyorTunnelPlaced = true;
            _firstPlacedTunnel = placedObject;
            _firstPlacedTunnel.ConveyorTunnelType = ConveyorTunnelType.Input;
        }
        else
        {
            (_firstPlacedTunnel.TunnelPlacedObject, placedObject.TunnelPlacedObject) = (placedObject, _firstPlacedTunnel);
            placedObject.ConveyorTunnelType = ConveyorTunnelType.Output;
            _firstPlacedTunnel = null;
            _isFirstConveyorTunnelPlaced = false;
            DeselectObjectType();
        }
    }
    private bool IsSecondTunnelValid(Vector2Int placedObjectOrigin)
    {
        var deltaX = placedObjectOrigin.x - _firstPlacedTunnel.Origin.x;
        var deltaY = placedObjectOrigin.y - _firstPlacedTunnel.Origin.y;

        return (_firstPlacedTunnel.Dir == Dir.Down && deltaY is >= -MaxTunnelLength and < 0 && deltaX == 0) ||
               (_firstPlacedTunnel.Dir == Dir.Left && deltaX is >= -MaxTunnelLength and < 0 && deltaY == 0) ||
               (_firstPlacedTunnel.Dir == Dir.Up && deltaY is <= MaxTunnelLength and > 0 && deltaX == 0) ||
               (_firstPlacedTunnel.Dir == Dir.Right && deltaX is <= MaxTunnelLength and > 0 && deltaY == 0);
    }
}