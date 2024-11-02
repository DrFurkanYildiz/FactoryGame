using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GridSystem;
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

    private Action<PlacedObjectType> _onSelectedPlacedObject;

    private void Awake()
    {
        Instance = this;
        _grid = InitializeGrid(20, 20, 1f);
        _pathfindingSystem = new PathfindingSystem<Tile>(_grid);

        _onSelectedPlacedObject += SelectObject;
    }
    private static Grid<Tile> InitializeGrid(int width, int height, float cellSize)
    {
        return new Grid<Tile>(width, height, cellSize, Vector3.zero, 
            (Grid<Tile> g, int x, int y) => new Tile(g, x, y));
    }


    private void Update()
    {
        HandleSelectObject();
        HandleRotateObject();
        HandleDemolish();
        
        
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

        _visual.position = Vector3.Lerp(_visual.position, targetPosition, Time.deltaTime * 25f);
        _visual.rotation = Quaternion.Lerp(_visual.rotation, GetPlacedObjectRotation(), Time.deltaTime * 25f);
    }

    private void RefreshVisual()
    {
        if (_visual != null) Destroy(_visual.gameObject);
        if (_placedObjectTypeSo == null) return;
        _visual = Instantiate(_placedObjectTypeSo.visual, GetMouseWorldSnappedPosition(), GetPlacedObjectRotation(),
            transform);
    }

    private bool _isFlat = true;
    private void ChangeBeltVisual()
    {
       
        var origin = _grid.GetCoordinate(Mouse3D.GetMouseWorldPosition());
        var currentTile = _grid.GetGridObject(origin);
        var currentNext = origin + PlacedObjectTypeSo.GetDirForwardVector(_dir);
        var currentPre = origin + PlacedObjectTypeSo.GetDirForwardVector(_dir) * -1;

        var neighbour = _pathfindingSystem.GetNeighbour(currentTile, false);
        ConveyorBelt preBelt = null;
        var i = 0;
        foreach (var tile in neighbour)
        {
            if (tile.OwnedObject is ConveyorBelt belt &&
                belt.NextPosition == currentTile.GetGridPosition && currentNext != tile.GetGridPosition &&
                currentPre != tile.GetGridPosition)
            {
                i++;
                preBelt = belt;
            }
        }

        if (i == 1)
        {
            Debug.Log(_dir + "Dön"  + preBelt.Dir);
            /*
            _isFlat = !_isFlat;
            _visual.GetChild(0).gameObject.SetActive(_isFlat);
            _visual.GetChild(1).gameObject.SetActive(!_isFlat);
            _visual.GetChild(1).GetChild(_dir == Dir.Left ? 0 : 1).gameObject.SetActive(true);
            */
        }
        else
        {
            //if (_isFlat) return;
            Debug.Log("Düz ol!");
            
            /*
            _isFlat = !_isFlat;
            
            _visual.GetChild(0).gameObject.SetActive(_isFlat);
            _visual.GetChild(1).gameObject.SetActive(!_isFlat);
            
            _visual.GetChild(1).GetChild(0).gameObject.SetActive(false);
            _visual.GetChild(1).GetChild(1).gameObject.SetActive(false);
           */
        }
        
    }

    private void HandleSelectObject()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) _onSelectedPlacedObject?.Invoke(PlacedObjectType.Storage);
        if (Input.GetKeyDown(KeyCode.Alpha2)) _onSelectedPlacedObject?.Invoke(PlacedObjectType.ConveyorBelt);
        if (Input.GetKeyDown(KeyCode.Alpha3)) _onSelectedPlacedObject?.Invoke(PlacedObjectType.ConveyorSplitter);
        if (Input.GetKeyDown(KeyCode.Alpha4)) _onSelectedPlacedObject?.Invoke(PlacedObjectType.Machine1);
        if (Input.GetKeyDown(KeyCode.Alpha5)) _onSelectedPlacedObject?.Invoke(PlacedObjectType.ConveyorTunnel);
        if (Input.GetKeyDown(KeyCode.Alpha6)) _onSelectedPlacedObject?.Invoke(PlacedObjectType.Test);
        
        
        //TODO: Deselect ve Demolish refaktör edilecek.
        if (Input.GetMouseButtonDown(1))
        {
            DeselectObjectType();
            if (_isFirstConveyorTunnelPlaced)
            {
                _isFirstConveyorTunnelPlaced = false;
                _firstPlacedTunnel.DestroySelf();
            }
        }
    }

    
    private void SelectObject(PlacedObjectType placedObjectType)
    {
        _placedObjectTypeSo = GetPlacedSo(placedObjectType);
        RefreshVisual();
    }
    
    private PlacedObjectTypeSo GetPlacedSo(PlacedObjectType type)
    {
        return placedObjectTypeSOList.Find(t => t.type == type);
    }
    
    private void HandleRotateObject()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _dir = PlacedObjectTypeSo.GetNextDir(_dir);
            
            
            if (_placedObjectTypeSo.type is PlacedObjectType.ConveyorBelt)
                ChangeBeltVisual();

        }
    }

    private void HandlePlacement()
    {
        if (Input.GetMouseButton(0))
        {
            if (_placedObjectTypeSo != null)
            {
                var mousePosition = Mouse3D.GetMouseWorldPosition();
                ObjectPlacement(_grid, _placedObjectTypeSo, mousePosition, _dir, out var placedObject);


                if (placedObject is ConveyorTunnel)
                {
                    ConveyorTunnelAssignment(placedObject);
                }
            }
        }
    }

    private void HandleDemolish()
    {
        if (Input.GetMouseButtonDown(1))
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