using UnityEngine;
using System.Collections.Generic;
using Gamelogic;
using Gamelogic.Grids;

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


			public FlatHexGrid<BattleCell> Grid{ get; set; }

			public IMap3D<FlatHexPoint> Map{ get; set; }

			public List<BattleCellState> State;

			public delegate void FlatHexDelegate (Vector3 _point, BattleCell _cell);

			public event FlatHexDelegate onClickCell;
			public event FlatHexDelegate onMouseOverCell;
			public event FlatHexDelegate onRightClickCell;


			#region Grid Building

			public void BuildGrid ()
			{
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

			#endregion


			#region Input

			void Update ()
			{
				getMouseClick ();
				mouseGridPosition ();
			}

			void getMouseClick ()
			{
				if (Input.GetMouseButtonDown (0))
				{
					onClickCell.Invoke (Map [mouseGridPoint], Grid [mouseGridPoint]);
				}

				if (Input.GetMouseButtonDown (1))
				{
					onRightClickCell.Invoke (Map [mouseGridPoint], Grid [mouseGridPoint]);

				}
			}

			void mouseGridPosition ()
			{
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
							mouseGridPoint = point;
							onMouseOverCell.Invoke (Map [point], Grid [mouseGridPoint]);
						}

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
			}

			public void UnRegisterObject (Vector3 _point)
			{
				var cell = Grid [Map [_point]];
				cell.context = CellContext.empty;
				cell.isAccessible = true;
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
	}
}
