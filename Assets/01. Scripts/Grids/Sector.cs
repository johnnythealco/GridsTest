using UnityEngine;
using System.Collections.Generic;
using Gamelogic;
using Gamelogic.Grids;

[RequireComponent (typeof(BoxCollider))]
public class Sector : GLMonoBehaviour
{

	public int size = 12;
	public Vector2 padding;
	public SectorCell battleCellPrefab;


	public static FlatHexGrid<SectorCell> Grid{ get; set; }

	public static IMap3D<FlatHexPoint> Map{ get; set; }

	#region Spawnpoints

	public List<FlatHexPoint> spawnpoints;
	public  FlatHexPoint centerSpawn;
	public  FlatHexPoint northSpawn;
	public  FlatHexPoint southSpawn;
	public  FlatHexPoint northEastSpwan;
	public  FlatHexPoint northWestSpawn;
	public  FlatHexPoint southEastSpwan;
	public  FlatHexPoint southWestSpawn;

	#endregion



	void Start ()
	{
		//		size = Battle.Manager.state.battleSectorSize;

		BuildGrid ();


	}


	#region Grid Building

	public void BuildGrid ()
	{

		var spacing = battleCellPrefab.Dimensions;
		spacing.Scale (padding);

		Grid = FlatHexGrid<SectorCell>.Hexagon (size);
		Map = new FlatHexMap (spacing).AnchorCellMiddleCenter ().To3DXZ ();
		foreach (var point in Grid)
		{
			var cell = Instantiate (battleCellPrefab);
			Vector3 worldPoint = Map [point];
			cell.transform.parent = this.transform;
			cell.transform.localScale = Vector3.one;
			cell.transform.localPosition = worldPoint;

			cell.name = point.ToString ();
			Grid [point] = cell;

		}
		positionCollider ();
		createSpawnpoints ();
	}

	void positionCollider ()
	{

		var gridDimensions = new Vector2 ((float)size * 2.1f, (float)size * 2.1f);

		gridDimensions.Scale (battleCellPrefab.Dimensions);

		Vector3 coliderSize = new Vector3 (gridDimensions.x, 0.1f, gridDimensions.y);

		this.GetComponent<BoxCollider> ().size = coliderSize;

	}

	void createSpawnpoints ()
	{
		int _spawnpointBuffer = size / 3;
		int _point = size - _spawnpointBuffer;

		centerSpawn = new FlatHexPoint (0, 0); 
		spawnpoints.Add (centerSpawn);
		northSpawn = new FlatHexPoint (0, _point);
		spawnpoints.Add (northSpawn);
		southSpawn = new FlatHexPoint (0, -_point);
		spawnpoints.Add (southSpawn);
		northEastSpwan = new FlatHexPoint (_point, 0);
		spawnpoints.Add (northEastSpwan);
		southEastSpwan = new FlatHexPoint (_point, -_point);
		spawnpoints.Add (southEastSpwan);
		northWestSpawn = new FlatHexPoint (-_point, _point); 
		spawnpoints.Add (northWestSpawn);
		southWestSpawn = new FlatHexPoint (-_point, 0);
		spawnpoints.Add (southWestSpawn);

		Grid [northSpawn].Color = Color.red;
		Grid [southSpawn].Color = Color.red;
		Grid [northEastSpwan].Color = Color.red;
		Grid [southEastSpwan].Color = Color.red;
		Grid [northWestSpawn].Color = Color.red;
		Grid [southWestSpawn].Color = Color.red;
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


		var _path = Algorithms.AStar<SectorCell, FlatHexPoint>
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


	#region Utility Methods

	//
	//	public static void registerAtPoint (Vector3 point, UnitController unit)
	//	{
	//		var _cell = Sector.Grid [Sector.Map [point]];
	//		_cell.contents = SectorCell.Contents.unit;
	//		_cell.unit = unit;
	//		_cell.isAccessible = false;
	//
	//		//		Battle.Manager.state.occupiedCells.Add (_cell);
	//
	//	}
	//
	//	public static void unRegisterAtPoint (Vector3 point, UnitController unit)
	//	{
	//		var _cell = Sector.Grid [Sector.Map [point]];
	//		_cell.contents = SectorCell.Contents.empty;
	//		_cell.unit = null;
	//		_cell.isAccessible = true;
	//
	//		//		Battle.Manager.state.occupiedCells.Remove (_cell);
	//
	//	}
	//
	//	public static void CreateUnit (Unit _unit, FlatHexPoint _point)
	//	{
	//		var template = Game.Manager.UnitRegister.Lookup (_unit.Designation);
	//		UnitController clone = Instantiate (template, Sector.Map [_point], Quaternion.identity) as UnitController;
	//
	//
	//		clone.unit = _unit;
	//
	//		Sector.Grid [_point].unit = clone;
	//
	//		Sector.Grid [_point].contents = SectorCell.Contents.unit;
	//		Sector.Grid [_point].isAccessible = false;
	//
	//		clone.unit.coordinates.x = _point.X;
	//		clone.unit.coordinates.y = _point.Y;
	//
	//
	//		Debug.Log ("Creating Unit at point: " + Sector.Grid [_point].name);
	//	}
	//
	//	public static List<Vector3> getDeploymentArea (FlatHexPoint point, int radius)
	//	{
	//		List<Vector3> result = new List<Vector3> ();
	//		var area = Algorithms.GetPointsInRange<SectorCell, FlatHexPoint>
	//		(Sector.Grid, point,
	//			           JKCell => JKCell.isAccessible,
	//			           (p, q) => 1,
	//			           radius
	//		           );
	//
	//
	//		foreach (var _point in area)
	//		{
	//			result.Add (Sector.Map [_point]);
	//		}
	//		return result;
	//	}
	//
	//	public static FlatHexPoint getPoint (GridPos _position)
	//	{
	//		return new FlatHexPoint (_position.x, _position.y);
	//	}
	//
	//	public void DeployFeet (List<Unit> fleet, List<Vector3> spwanPoints)
	//	{
	//		for (int i = 0; i < fleet.Count; i++)
	//		{
	//			CreateUnit (fleet [i], Map [spwanPoints [i]]);
	//		}
	//	}
	//
	//	public void LoadNewBattle ()
	//	{
	//		for (int i = 0; i < Game.Manager.Players.Count; i++)
	//		{
	//			var fleet = Game.Manager.Players [i].fleet;
	//			var deploymentArea = getDeploymentArea (spawnpoints [i], fleet.Count);
	//			DeployFeet (fleet, deploymentArea);
	//		}
	//	}

	#endregion




}
