﻿using UnityEngine;
using System.Collections.Generic;
using Gamelogic;
using Gamelogic.Grids;
using System.Linq;

namespace JK
{
	namespace Grids
	{

		[RequireComponent (typeof(BoxCollider))]
		public class BattleGrid : GLMonoBehaviour
		{
            #region Variables

            #region Settings
            [SerializeField]
			int size;
            [SerializeField]
			BattleCell flatHexCell;
            [SerializeField]
            int spawnPointBuffer;
            [SerializeField]
            int deploymentAreaSize;
            #endregion

            #region Private
            FlatHexPoint mouseGridPoint;
			FlatHexPoint newMouseGridPoint;
			bool mouseOverGrid;
            #endregion

            #region Public
            public FlatHexGrid<BattleCell> Grid{ get; set; }

			public IMap3D<FlatHexPoint> Map{ get; set; }
            #endregion

            #region Static
            public static List<Vector3> Spawnpoints;
            public static List<List<Vector3>> DeploymentAreas;

            public static Vector3 CenterSpawn;
            public static Vector3 NorthSpawn;
            public static Vector3 SouthSpawn;
            public static Vector3 NorthEastSpawn;
            public static Vector3 SouthEastSpawn;
            public static Vector3 NorthWestSpawn;
            public static Vector3 SouthWestSpawn;

            public static List<Vector3> CenterDeploymentArea;
            public static List<Vector3> NorthDeploymentArea;
            public static List<Vector3> SouthDeploymentArea;
            public static List<Vector3> NorthEastDeploymentArea;
            public static List<Vector3> SouthEastDeploymentArea;
            public static List<Vector3> NorthWestDeploymentArea;
            public static List<Vector3> SouthWestDeploymentArea;
            #endregion

            #endregion

            #region Delegates & Events

            public delegate void FlatHexDelegate_point_cell (Vector3 _point);

			public delegate void FlatHexDelegate ();

			public event FlatHexDelegate_point_cell onClickCell;
			public event FlatHexDelegate_point_cell onMouseOverCell;
			public event FlatHexDelegate_point_cell onRightClickCell;
			public event FlatHexDelegate onNoCellSelected;

            #endregion

            #region Grid Building

            public void Setup()
            {
                BuildGrid();
                positionCollider();
                CreateSpawnPoints(spawnPointBuffer, deploymentAreaSize);


            }

            public void BuildGrid ()
			{
				Game.GridPoints = new List<Vector3> ();
				Battle.AllUnits = new List<Unit> ();
				var padding = new Vector2 (1.1f, 1.1f);
				var spacing = flatHexCell.Dimensions;
				spacing.Scale (padding);

				Grid = FlatHexGrid<BattleCell>.Hexagon (size);
				Map = new FlatHexMap (spacing).AnchorCellMiddleCenter ().To3DXZ ();
				foreach (var point in Grid)
				{
					var cell = Instantiate (flatHexCell);
					Vector3 worldPoint = Map [point];
					cell.transform.parent = this.transform;
					cell.transform.localScale = Vector3.one;
					cell.transform.localPosition = worldPoint;
					Game.GridPoints.Add (worldPoint);

					cell.name = point.ToString ();
					Grid [point] = cell;
                    


				}
				positionCollider ();
			}

			void positionCollider ()
			{

				var gridDimensions = new Vector2 ((float)size * 2.1f, (float)size * 2.1f);

				gridDimensions.Scale (flatHexCell.Dimensions);

				Vector3 coliderSize = new Vector3 (gridDimensions.x, 0.1f, gridDimensions.y);

				this.GetComponent<BoxCollider> ().size = coliderSize;

			}


			#endregion

			#region Pathfinding

			/**
	 * Returns a list of Flat Hex Points including the start and end points
	 * accounting for isAccesible and cost for each cell along the path 
	 * */
			public List<FlatHexPoint> getGridPath (FlatHexPoint start, FlatHexPoint end)
			{
				List<FlatHexPoint> path = new List<FlatHexPoint> ();



				var _path = Algorithms.AStar<BattleCell, FlatHexPoint>
		(Grid, start, end,
					            (p, q) => p.DistanceFrom (q),
					            c => c.isAccessible,
					            (p, q) => (Grid [p].Cost + Grid [q].Cost / 2)
				            );

				foreach (var step in _path)
				{
					path.Add (step); 

				}

				return path;
			}

			/**
	 * Returns a list of Vector3s including the start and end Vector3s
	 * accounting for isAccesible and cost for each cell along the path 
	 * */
			public List<Vector3> getGridPath (Vector3 start, Vector3 end)
			{
				List<Vector3> path = new List<Vector3> ();


				var _start = Map [start];
				var _end = Map [end];

                var _path = getGridPath(_start, _end);                

				foreach (var step in _path)
				{                
					path.Add (Map [step]);
				}

                return path;
			}

            public bool PathBlocked_LargeUnit(List<Vector3> _path)
            {      

                foreach(var worldpoint in _path)
                {
                    var step = Map[worldpoint];
                    var neighbours = GetNeighbours(step);

                    foreach(var point in neighbours)
                    {
                        if (Grid[point].isAccessible == false)
                            return true;
                    }
                }
                return false;

            }

            public List<Vector3> getPathToTarget (Vector3 start, Vector3 end)
			{
				List<Vector3> path = new List<Vector3> ();
				var _start = Map [start];
				var _end = Map [end];

				Grid [_end].isAccessible = true;

				var _path = getGridPath (_start, _end);

				foreach (var step in _path)
				{
					path.Add (Map [step]);

				}

				var target = path.Last (); 
				var source = path.First ();
				path.Remove (target);
				path.Remove (source);

				Debug.Log (" Path Lenght : " + path.Count () + " Steps.");

				Grid [_end].isAccessible = false;

				return path;
			}

			#endregion

			#region Input

			void Update ()
			{
				getMouseClick ();
				mouseGridPosition ();
			}

			void getMouseClick ()
			{
				if (Input.GetMouseButtonDown (0) && mouseOverGrid)
				{
					onClickCell.Invoke (Map [mouseGridPoint]);
				}

				if (Input.GetMouseButtonDown (1) & mouseOverGrid)
				{
					onRightClickCell.Invoke (Map [mouseGridPoint]);

				}
			}

			void mouseGridPosition ()
			{
				//If the mouse is ove a UI Element
				if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject ())
				{
					if (mouseOverGrid)
					{
						mouseOverGrid = false;
						mouseGridPoint = new FlatHexPoint ();
						onNoCellSelected.Invoke ();
					}
					return;
				}
					

				//Determin What Cell the mouse is currently over
				var ray = Battle.ActiveBattleCam.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast (ray, out hit))
				{
					Vector3 worldPosition = this.transform.InverseTransformPoint (hit.point);
					var point = Map [worldPosition];

					if (point == mouseGridPoint)
						return;
					
					if (Grid.Contains (point))
					{
						{
							mouseOverGrid = true;
							mouseGridPoint = point;
							onMouseOverCell.Invoke (Map [point]);
						}

						//if the mouse is not over the grid
					} else if (mouseOverGrid)
					{
						mouseOverGrid = false;
						mouseGridPoint = new FlatHexPoint ();
						onNoCellSelected.Invoke ();
					}
				}
			}

            #endregion

            #region Utility Methods

            public void LoadState(BattleState _BattleState)
            {
                foreach (var _cellState in _BattleState.Units)
                {
                    var _point = Map[_cellState.position];
                    var _cell = Grid[_point];
                    _cell.SetState(_cellState);
                }

                foreach (var _point in _BattleState.TurnOrder)
                {
                    var _cell = Game.BattleManager.battleGrid.GetCell(_point);
                    Battle.AllUnits.Add(_cell.unit);

                }
            }

            public List<FlatHexPoint> GetNeighbours(FlatHexPoint _gridpoint)
            {
                List<FlatHexPoint> result = new List<FlatHexPoint>();

                FlatHexPoint North = new FlatHexPoint(_gridpoint.X, _gridpoint.Y + 1);               
                FlatHexPoint South = new FlatHexPoint(_gridpoint.X, _gridpoint.Y - 1);               
                FlatHexPoint NorthEast = new FlatHexPoint(_gridpoint.X + 1, _gridpoint.Y + 0);
                FlatHexPoint NorthWest = new FlatHexPoint(_gridpoint.X - 1, _gridpoint.Y + 1);               
                FlatHexPoint SouthWest = new FlatHexPoint(_gridpoint.X - 1, _gridpoint.Y);               
                FlatHexPoint SouthEast = new FlatHexPoint(_gridpoint.X + 1, _gridpoint.Y - 1);

                if(Grid.Contains(North))
                    result.Add(North);

                if (Grid.Contains(South))
                    result.Add(South);

                if (Grid.Contains(NorthEast))
                    result.Add(NorthEast);

                if (Grid.Contains(NorthWest))
                    result.Add(NorthWest);

                if (Grid.Contains(SouthWest))
                    result.Add(SouthWest);

                if (Grid.Contains(SouthEast))
                    result.Add(SouthEast);

                return result;


            }
            
         	public BattleCell GetCell (Vector3 _point)
			{
				return Grid [Map [_point]];
			}
            	
			public bool GetCellAccessiblity (Vector3 _point)
			{
				var cell = Grid [Map [_point]];
				return cell.isAccessible;
			}

			public void RegisterUnit (Vector3 _point, Unit _unit, CellContents _contents)
			{
				var cell = Grid [Map [_point]];
				cell.unit = _unit;
				cell.contents = _contents;
				cell.isAccessible = false;			

				if (Battle.AllUnits.Contains (_unit) == false)
				{
					Battle.AllUnits.Add (_unit);
				}

                if(_unit.Size == unitSize.large)
                {
                    var neighbors = GetNeighbours(Map[_point]);

                    foreach(var fhp in neighbors)
                    {

                        Grid[fhp].unit = _unit;
                        Grid[fhp].contents = _contents;
                        Grid[fhp].isAccessible = false;
                    }
                }

			}

            public void UnRegisterObject(Vector3 _point)
            {
                var cell = Grid[Map[_point]];
                var _unit = cell.unit;

                cell.contents = CellContents.empty;
                cell.unit = null;
                cell.isAccessible = true;

                if (_unit.Size == unitSize.large)
                {
                    var neighbors = GetNeighbours(Map[_point]);

                    foreach (var fhp in neighbors)
                    {
                        Grid[fhp].contents = CellContents.empty;
                        Grid[fhp].unit = null;
                        Grid[fhp].isAccessible = true;
                    }

                }
            }

			public List<Vector3> GetRange (Vector3 _point, int _radius)
			{
                var gridPoint = Map[_point];

                List<Vector3> result = new List<Vector3>();

                var area = Algorithms.GetPointsInRange<BattleCell, FlatHexPoint>
                                (Grid, gridPoint,
                               JKCell => JKCell.isAccessible,
                               (p, q) => 1,
                               _radius
                           );
                foreach (var _gridPoint in area)
                {
                    var worldpoint = Map[_gridPoint];
                    result.Add(worldpoint);
                }
                result.Remove(_point);

                return result;

			}

            public List<Vector3> GetMovementRange(Vector3 _point, int _radius)
            {

                var gridPoint = Map[_point];
                var cell = Grid[gridPoint];
                var _unit = cell.unit;

                List<Vector3> result = new List<Vector3>();

                if (_unit == null)
                    return null;

                if (_unit.Size == unitSize.large)
                {
                    SetLargeUnitAccessiblty(_point, true);
                }
                var area = Algorithms.GetPointsInRange<BattleCell, FlatHexPoint>
                                (Grid, gridPoint,
                               JKCell => JKCell.isAccessible,
                               (p, q) => 1,
                               _radius
                           );
                foreach (var _gridPoint in area)
                {
                    var worldpoint = Map[_gridPoint];
                    result.Add(worldpoint);
                }
                result.Remove(_point);

                if (_unit.Size == unitSize.large)
                {
                    SetLargeUnitAccessiblty(_point, true);
                }

                return result;
            }

            public int GetDistance(Vector3 _start, Vector3 _destination)
            {
                return Map[_start].DistanceFrom(Map[_destination]);
            }

            public List<Vector3> GetTargets(Vector3 _Source, int _range, TargetType _targetType)
            {
                
                var sourceGridPoint = Map[_Source];

               var validTargets = new List<Vector3>();

                foreach (var _unit in Battle.AllUnits)
                {
                    if (_unit.targetType == _targetType)
                    {
                       if (Game.BattleManager.ActiveUnit.Size == unitSize.large)
                            SetLargeUnitAccessiblty(Game.BattleManager.ActiveUnit.transform.position, true);

                        if (_unit.Size == unitSize.large)
                            SetLargeUnitAccessiblty(_unit.transform.position, true);



                        var _targetGridPoint = Map[_unit.transform.position];
                        var _path = Map.GetLine(sourceGridPoint, _targetGridPoint);
                        _path.Remove(_targetGridPoint);
                        _path.Remove(Map[_Source]);

                        if (_path.Count() == 0)
                        {
                            validTargets.Add(_unit.transform.position);
                        }

                        if (_path.Count() <= _range)
                        {
                            foreach (var step in _path)
                            {
                                //if the path is blocked break out of the loop and go to check next unit;
                                if (Grid[step].isAccessible == false)
                                    break;

                                validTargets.Add(_unit.transform.position);
                            }
                        }

                        if (Game.BattleManager.ActiveUnit.Size == unitSize.large)
                            SetLargeUnitAccessiblty(Game.BattleManager.ActiveUnit.transform.position, false);

                        if (_unit.Size == unitSize.large)
                            SetLargeUnitAccessiblty(_unit.transform.position, false);

                    }

                }                
                return validTargets;
            }

            public void CreateSpawnPoints(int _buffer, int _deploymentAreaSize)
            {
               
                int _point = size - _buffer;

                CenterSpawn = Map[new FlatHexPoint(0, 0)];
                CenterDeploymentArea = GetRange(CenterSpawn, _deploymentAreaSize);

                NorthSpawn = Map[new FlatHexPoint(0, _point)];
                NorthDeploymentArea = GetRange(NorthSpawn, _deploymentAreaSize);

                SouthSpawn = Map[new FlatHexPoint(0, -_point)];
                SouthDeploymentArea = GetRange(SouthSpawn, _deploymentAreaSize);

                NorthEastSpawn = Map[new FlatHexPoint(_point, 0)];
                NorthEastDeploymentArea = GetRange(NorthEastSpawn, _deploymentAreaSize);

                SouthEastSpawn = Map[new FlatHexPoint(_point, -_point)];
                SouthEastDeploymentArea = GetRange(SouthEastSpawn, _deploymentAreaSize);

                NorthWestSpawn = Map[new FlatHexPoint(-_point, _point)];
                NorthWestDeploymentArea = GetRange(NorthWestSpawn, _deploymentAreaSize);

                SouthWestSpawn = Map[new FlatHexPoint(-_point, 0)];
                SouthWestDeploymentArea = GetRange(SouthWestSpawn, _deploymentAreaSize);

                Spawnpoints = new List<Vector3>();
                Spawnpoints.Add(CenterSpawn);
                Spawnpoints.Add(NorthSpawn);
                Spawnpoints.Add(SouthSpawn);
                Spawnpoints.Add(NorthEastSpawn);
                Spawnpoints.Add(SouthEastSpawn);
                Spawnpoints.Add(NorthWestSpawn);
                Spawnpoints.Add(SouthWestSpawn);

                DeploymentAreas = new List<List<Vector3>>();
                DeploymentAreas.Add(CenterDeploymentArea);
                DeploymentAreas.Add(NorthDeploymentArea);
                DeploymentAreas.Add(SouthDeploymentArea);
                DeploymentAreas.Add(NorthEastDeploymentArea);
                DeploymentAreas.Add(SouthEastDeploymentArea);
                DeploymentAreas.Add(NorthWestDeploymentArea);
                DeploymentAreas.Add(SouthWestDeploymentArea);


        }

            public void SetLargeUnitAccessiblty(Vector3 _point,bool _value)
            {
                var _gridPoint = Map[_point];

                Grid[_gridPoint].isAccessible = _value;

                var _neighbours = GetNeighbours(_gridPoint);

                foreach(var _neighbour in _neighbours)
                {
                    Grid[_neighbour].isAccessible = _value;
                }

            }

            public bool getNeighboursAccessibility(Vector3 _point)
            {
                var _gridpoint = Map[_point];
                var _neighbours = GetNeighbours(_gridpoint);

                foreach(var _neighbour in _neighbours)
                {
                    if (Grid[_neighbour].isAccessible == false)
                        return false;
                }
                return true;
            }

            #endregion

        }
        
	}
}
