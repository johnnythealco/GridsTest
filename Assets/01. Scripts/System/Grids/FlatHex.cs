using UnityEngine;
using System.Collections.Generic;
using Gamelogic;
using Gamelogic.Grids;
using System.Linq;

namespace JK
{
	namespace Grids
	{

		[RequireComponent (typeof(BoxCollider))]
		public class FlatHex : GLMonoBehaviour
		{

			public int size;
			Vector2 padding;
			public BattleCell flatHexCell;

			FlatHexPoint mouseGridPoint;
			FlatHexPoint newMouseGridPoint;
			bool mouseOverGrid;

			public FlatHexGrid<BattleCell> Grid{ get; set; }

			public IMap3D<FlatHexPoint> Map{ get; set; }

			public List<BattleCellState> State;

			public List<UnitModel> units;

			#region Delegates & Events

			public delegate void FlatHexDelegate_point_cell (Vector3 _point, BattleCell _cell);

			public delegate void FlatHexDelegate ();

			public event FlatHexDelegate_point_cell onClickCell;
			public event FlatHexDelegate_point_cell onMouseOverCell;
			public event FlatHexDelegate_point_cell onRightClickCell;
			public event FlatHexDelegate onNoCellSelected;

			#endregion

			public List<BattleCell> occupiedCells = new List<BattleCell> ();



			#region Grid Building

			public void BuildGrid ()
			{
				Game.GridPoints = new List<Vector3> ();
				Battle.AllUnits = new List<UnitModel> ();
				size = 12;
				padding = new Vector2 (1.1f, 1.1f);
				var spacing = flatHexCell.Dimensions;
				spacing.Scale (padding);

				Grid = FlatHexGrid<BattleCell>.Hexagon (size);
				Map = new FlatHexMap (spacing).AnchorCellMiddleCenter ().To3DXZ ();
				State = new List<BattleCellState> ();
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

				var _path = getGridPath (_start, _end);

				foreach (var step in _path)
				{
					path.Add (Map [step]);

				}

				return path;
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
					onClickCell.Invoke (Map [mouseGridPoint], Grid [mouseGridPoint]);
				}

				if (Input.GetMouseButtonDown (1) & mouseOverGrid)
				{
					onRightClickCell.Invoke (Map [mouseGridPoint], Grid [mouseGridPoint]);

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
				var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
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
							onMouseOverCell.Invoke (Map [point], Grid [mouseGridPoint]);
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

			public void LogVector3ToGridPoint (Vector3 _point)
			{
				Debug.Log ("Grid Point : " + Map [_point].ToString ());
			}

			public BattleCell GetCell (Vector3 _point)
			{
				return Grid [Map [_point]];
			}



			public CellContext GetCellContents (Vector3 _point)
			{
				var cell = Grid [Map [_point]];
				return cell.context;
			}

			public bool GetCellAccessiblity (Vector3 _point)
			{
				var cell = Grid [Map [_point]];
				return cell.isAccessible;
			}

			public void RegisterUnit (Vector3 _point, UnitModel _unit, CellContext _contents)
			{
				var cell = Grid [Map [_point]];
				cell.unit = _unit;
				cell.context = _contents;
				cell.isAccessible = false;

				occupiedCells.Add (cell);

				if (Battle.AllUnits.Contains (_unit) == false)
				{
					Battle.AllUnits.Add (_unit);
				}
			}

			public void UnRegisterObject (Vector3 _point)
			{
				var cell = Grid [Map [_point]];
				cell.context = CellContext.empty;
				cell.isAccessible = true;
				occupiedCells.Remove (cell);

			}

			public List<Vector3> GetRange (Vector3 _point, int _radius)
			{
				var point = Map [_point];
				List<Vector3> result = new List<Vector3> ();
				var area = Algorithms.GetPointsInRange<BattleCell, FlatHexPoint>
								(Grid, point,
					           JKCell => JKCell.isAccessible,
					           (p, q) => 1,
					           _radius
				           );
				foreach (var _gridPoint in area)
				{
					var worldpoint = Map [_gridPoint];
					result.Add (worldpoint);
				}
				result.Remove (_point);
				return result;
			}


			#endregion

		}

		#region Spawnpoints
		public class SpawnPoint
		{
			public string name;
			public Vector3 point;

			public SpawnPoint (string _name, Vector3 _point)
			{
				this.name = _name;
				this.point = _point;
			}
		}

		public class SpawnPoints
		{
			public List<SpawnPoint> list = new List<SpawnPoint> ();

		
			/// <summary>
			/// Initializes a new instance of the SpawnPoints class.
			/// Reccommended buffer or 3.
			/// </summary>
			/// <param name="_Grid">Grid.</param>
			/// <param name="_buffer">Buffer.</param>
			public SpawnPoints (FlatHex _Grid, int _buffer)
			{
				int _spawnpointBuffer = _Grid.size / _buffer;
				int _point = _Grid.size - _spawnpointBuffer;
		
				var centerSpawn = new SpawnPoint ("Center", _Grid.Map [new FlatHexPoint (0, 0)]);
				var northSpawn = new SpawnPoint ("North", _Grid.Map [new FlatHexPoint (0, _point)]);
				var southSpawn = new SpawnPoint ("South", _Grid.Map [new FlatHexPoint (0, -_point)]);
				var northEastSpwan = new SpawnPoint ("NorthEast", _Grid.Map [new FlatHexPoint (_point, 0)]);
				var southEastSpwan = new SpawnPoint ("SouthEast", _Grid.Map [new FlatHexPoint (_point, -_point)]);
				var northWestSpawn = new SpawnPoint ("NorthWest", _Grid.Map [new FlatHexPoint (-_point, _point)]);
				var southWestSpawn = new SpawnPoint ("SouthWest", _Grid.Map [new FlatHexPoint (-_point, 0)]);
				list.Add (centerSpawn);
				list.Add (northSpawn);
				list.Add (southSpawn);
				list.Add (northEastSpwan);
				list.Add (southEastSpwan);
				list.Add (northWestSpawn);
				list.Add (southWestSpawn);
		
		
			}
		}
		#endregion
	}
}
