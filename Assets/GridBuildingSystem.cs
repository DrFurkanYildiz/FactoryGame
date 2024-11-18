using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GridSystem;
using Helpers;
using UnityEngine;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }

    private Grid<Tile> _grid;
    private PlaceableObjectBaseSo _placeableObjectSo;
    [SerializeField] private Dir _dir;
    private Transform _visual;

    //public bool isDemolishActive;

    private bool _isFirstConveyorTunnelPlaced;
    private PlaceableObjectBase _firstPlaceableTunnel;
    private const int MaxTunnelLength = 5;

    private ConveyorBeltVisualController.BeltVisualDirection _beltVisualDirection;
    private Vector2Int _conveyorBeltChangedVisualCoordinate;

    public PathfindingSystem<Tile> PathfindingSystem { get; private set; }

    private Action<PlaceableType> _onSelectedPlacedObject;

    private bool _rotateLocked;
    private int _lockedIndex;

    private void Awake()
    {
        Instance = this;
        _grid = InitializeGrid(20, 20, 1f);
        PathfindingSystem = new PathfindingSystem<Tile>(_grid);

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

        if (_placeableObjectSo != null)
        {
            if (_placeableObjectSo.type is not PlaceableType.ConveyorBelt)
            {
                _rotateLocked = false;
                _lockedIndex = 0;
                return;
            }
            
            
            var mouseGridPosition = _grid.GetCoordinate(Mouse3D.GetMouseWorldPosition());
            var belt = _visual.GetComponent<ConveyorBeltVisualController>();

            if (mouseGridPosition != _conveyorBeltChangedVisualCoordinate)
            {
                if (belt.direction is not ConveyorBeltVisualController.BeltVisualDirection.Flat)
                    belt.SetVisualDirection(ConveyorBeltVisualController.BeltVisualDirection.Flat);
                _beltVisualDirection = ConveyorBeltVisualController.BeltVisualDirection.Flat;
            }

            var currentTile = _grid.GetGridObject(mouseGridPosition);
            var list = currentTile.GetNeighbourConveyorBelt(PathfindingSystem);

            switch (list.Count)
            {
                case 1:
                    UpdateConveyorBeltVisual(list, 0);
                    break;
                case 2:
                    var previousPosition = mouseGridPosition + PlaceableObjectBaseSo.GetDirForwardVector(_dir) * -1;
                    var nextPosition = mouseGridPosition + PlaceableObjectBaseSo.GetDirForwardVector(_dir);
                    var a = list.Count(b => b.Origin == previousPosition || b.Origin == nextPosition);

                    if (a <= 1)
                    {
                        _rotateLocked = true;
                        if (Input.GetKeyDown(KeyCode.R))
                            _lockedIndex = (_lockedIndex + 1) % list.Count;

                        UpdateConveyorBeltVisual(list, _lockedIndex);
                        return;
                    }

                    goto case 1;

                case 3: goto case 2;
            }

            _rotateLocked = false;
            _lockedIndex = 0;
        }
    }

    private void UpdateConveyorBeltVisual(IReadOnlyList<ConveyorBelt> list, int i)
    {
        var mouseGridPosition = _grid.GetCoordinate(Mouse3D.GetMouseWorldPosition());
        var belt = _visual.GetComponent<ConveyorBeltVisualController>();
        var dir = belt.GetVisualDirection(_dir, list[i].Dir);
        if (dir == belt.direction) return;

        belt.SetVisualDirection(dir);
        _beltVisualDirection = dir;
        _conveyorBeltChangedVisualCoordinate = mouseGridPosition;
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
        if (_placeableObjectSo == null) return;
        _visual = Instantiate(_placeableObjectSo.visual, GetMouseWorldSnappedPosition(), GetPlacedObjectRotation(),
            transform);
    }

    private void HandleSelectObject()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) _onSelectedPlacedObject?.Invoke(PlaceableType.Storage);
        if (Input.GetKeyDown(KeyCode.Alpha2)) _onSelectedPlacedObject?.Invoke(PlaceableType.ConveyorBelt);
        if (Input.GetKeyDown(KeyCode.Alpha3)) _onSelectedPlacedObject?.Invoke(PlaceableType.Splitter);
        if (Input.GetKeyDown(KeyCode.Alpha4)) _onSelectedPlacedObject?.Invoke(PlaceableType.Machine1);
        if (Input.GetKeyDown(KeyCode.Alpha5)) _onSelectedPlacedObject?.Invoke(PlaceableType.ConveyorTunnel);


        //TODO: Deselect ve Demolish refaktör edilecek.
        if (Input.GetMouseButtonDown(1))
        {
            DeselectObjectType();
            if (_isFirstConveyorTunnelPlaced)
            {
                _isFirstConveyorTunnelPlaced = false;
                _firstPlaceableTunnel.DestroySelf();
            }
        }
    }


    private void SelectObject(PlaceableType placeableType)
    {
        _placeableObjectSo = GameAssets.i.GetPlacedSo(placeableType);
        RefreshVisual();
    }

    private void HandleRotateObject()
    {
        if (Input.GetKeyDown(KeyCode.R) && !_rotateLocked)
        {
            _dir = PlaceableObjectBaseSo.GetNextDir(_dir);
        }
    }

    private void HandlePlacement()
    {
        if (Input.GetMouseButton(0))
        {
            if (_placeableObjectSo != null)
            {
                var mousePosition = Mouse3D.GetMouseWorldPosition();
                
                ObjectPlacement(_grid, _placeableObjectSo, mousePosition, _dir, out var placedObject);

                if (placedObject is ConveyorBelt currentBelt)
                {
                    currentBelt.transform.SetParent(GameObject.Find("Belts").transform);
                    currentBelt.BeltVisual.SetVisualDirection(_beltVisualDirection);
                    _beltVisualDirection = ConveyorBeltVisualController.BeltVisualDirection.Flat;


                    foreach (var tile in PathfindingSystem.GetNeighbour(_grid.GetGridObject(mousePosition)))
                    {
                        if (tile.OwnedObjectBase is ConveyorBelt neighbourBelt)
                        {
                            if (currentBelt.NextPosition == neighbourBelt.Origin)
                            {
                                if (neighbourBelt.GetNeighbourBelt().Count == 1)
                                {
                                    var dir = neighbourBelt.BeltVisual.GetVisualDirection(neighbourBelt.Dir, _dir);
                                    neighbourBelt.BeltVisual.SetVisualDirection(dir);
                                }
                            }
                        }
                    }
                }

                if (placedObject is ConveyorTunnel)
                {
                    ConveyorTunnelAssignment(placedObject);
                }
            }
        }
    }

    private void HandleDemolish()
    {
        if (Input.GetMouseButton(1))
        {
            var currentTile = _grid.GetGridObject(Mouse3D.GetMouseWorldPosition());
            var placedObject = currentTile.OwnedObjectBase;

            if (placedObject == null) return;
            DestroyPlacedObject(placedObject);
        }
    }

    private void DestroyPlacedObject(PlaceableObjectBase placedObject)
    {
        placedObject.DestroySelf();

        // Clear the grid position data
        var gridPositionList = placedObject.GetGridPositionList();
        foreach (var gridPosition in gridPositionList)
            _grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();

        if (placedObject is ConveyorBelt currentBelt)
        {
            foreach (var tile in PathfindingSystem.GetNeighbour(placedObject.GetTile))
            {
                if (tile.OwnedObjectBase is ConveyorBelt neighbourBelt)
                {
                    if (currentBelt.NextPosition == neighbourBelt.Origin)
                    {
                        var nList = neighbourBelt.GetNeighbourBelt();
                        if (nList.Count > 0)
                        {
                            var dir = neighbourBelt.BeltVisual.GetVisualDirection(neighbourBelt.Dir, nList[0].Dir);
                            neighbourBelt.BeltVisual.SetVisualDirection(dir);
                        }
                        else
                        {
                            if (neighbourBelt.BeltVisual.direction != ConveyorBeltVisualController.BeltVisualDirection.Flat)
                                neighbourBelt.BeltVisual.SetVisualDirection(ConveyorBeltVisualController.BeltVisualDirection.Flat);
                        }
                    }
                }
            }
        }
    }


    private void ObjectPlacement(Grid<Tile> grid, PlaceableObjectBaseSo placeableObjectSo, Vector3 selectPosition,
        Dir dir, out PlaceableObjectBase placeableObjectBase)
    {
        grid.GetXZ(selectPosition, out var x, out var z);
        var placedObjectOrigin = new Vector2Int(x, z);

        var gridPositionList = placeableObjectSo.GetGridPositionList(placedObjectOrigin, dir);
        var canBuild =
            gridPositionList.All(gridPosition => grid.GetGridObject(gridPosition.x, gridPosition.y).canBuild);

        if (!canBuild)
        {
            placeableObjectBase = null;
            return;
        }

        var rotationOffset = placeableObjectSo.GetRotationOffset(dir);
        var placedObjectWorldPosition = grid.GetWorldPosition(x, z) +
                                        new Vector3(rotationOffset.x, 0, rotationOffset.y) *
                                        grid.GetCellSize();
        placeableObjectBase =
            PlaceableObjectBase.Create(grid, placedObjectWorldPosition, placedObjectOrigin, dir, placeableObjectSo);

        foreach (var gridPosition in gridPositionList)
            grid.GetGridObject(gridPosition.x, gridPosition.y).OwnedObjectBase = placeableObjectBase;
    }


    private void DeselectObjectType()
    {
        _placeableObjectSo = null;
        RefreshVisual();
    }


    private Vector3 GetMouseWorldSnappedPosition()
    {
        if (_placeableObjectSo == null) return Mouse3D.GetMouseWorldPosition();
        Vector2Int rotationOffset = _placeableObjectSo.GetRotationOffset(_dir);
        Vector3 placedObjectWorldPosition = _grid.GetWorldPosition(Mouse3D.GetMouseWorldPosition()) +
                                            new Vector3(rotationOffset.x, 0, rotationOffset.y) * _grid.GetCellSize();
        return placedObjectWorldPosition;
    }

    private Vector3 GetMouseWorldSnappedPosition(PlaceableObjectSo placeableObjectSo, Vector3 position, Dir dir)
    {
        var rotationOffset = placeableObjectSo.GetRotationOffset(dir);
        return _grid.GetWorldPosition(position) +
               new Vector3(rotationOffset.x, 0, rotationOffset.y) * _grid.GetCellSize();
    }

    private Quaternion GetPlacedObjectRotation()
    {
        return _placeableObjectSo != null
            ? Quaternion.Euler(0, PlaceableObjectSo.GetRotationAngle(_dir), 0)
            : Quaternion.identity;
    }

    private Quaternion GetPlacedObjectRotation(Dir dir)
    {
        return Quaternion.Euler(0, PlaceableObjectSo.GetRotationAngle(dir), 0);
    }

    private List<Vector3> FindBeltPath(Vector2Int start, Vector2Int end)
    {
        var path = PathfindingSystem.FindPath(_grid.GetWorldPosition(start), _grid.GetWorldPosition(end), false);
        if (path == null) Debug.Log("Path not found!"); //Debug.DrawLine(path[i], path[i + 1], Color.red, 1f);
        return path;
    }

    private void ConveyorTunnelAssignment(PlaceableObjectBase placeableObjectBase)
    {
        if (!_isFirstConveyorTunnelPlaced)
        {
            _isFirstConveyorTunnelPlaced = true;
            _firstPlaceableTunnel = placeableObjectBase;
            _firstPlaceableTunnel.ConveyorTunnelType = ConveyorTunnelType.Input;
        }
        else
        {
            (_firstPlaceableTunnel.TunnelPlaceableObjectBase, placeableObjectBase.TunnelPlaceableObjectBase) =
                (placeableObjectBase, _firstPlaceableTunnel);
            placeableObjectBase.ConveyorTunnelType = ConveyorTunnelType.Output;
            _firstPlaceableTunnel = null;
            _isFirstConveyorTunnelPlaced = false;
            DeselectObjectType();
        }
    }

    private bool IsSecondTunnelValid(Vector2Int placedObjectOrigin)
    {
        var deltaX = placedObjectOrigin.x - _firstPlaceableTunnel.Origin.x;
        var deltaY = placedObjectOrigin.y - _firstPlaceableTunnel.Origin.y;

        return (_firstPlaceableTunnel.Dir == Dir.Down && deltaY is >= -MaxTunnelLength and < 0 && deltaX == 0) ||
               (_firstPlaceableTunnel.Dir == Dir.Left && deltaX is >= -MaxTunnelLength and < 0 && deltaY == 0) ||
               (_firstPlaceableTunnel.Dir == Dir.Up && deltaY is <= MaxTunnelLength and > 0 && deltaX == 0) ||
               (_firstPlaceableTunnel.Dir == Dir.Right && deltaX is <= MaxTunnelLength and > 0 && deltaY == 0);
    }
}