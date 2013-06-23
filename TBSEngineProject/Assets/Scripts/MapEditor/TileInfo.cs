using UnityEngine;
using System.Collections;

public class TileInfo {
	public int GridX { get; set; }
	public int GridY { get; set; }
	public string TilesetName { get; set; }
	public int TilesetX { get; set; }
	public int TilesetY { get; set; }
	public bool Passable { get; set; }
	public int Depth { get; set; }
	
	public TileInfo(int x, int y, string tilesetName, int tX, int tY, bool passable = true, int depth = 1) {
		GridX = x;
		GridY = y;
		TilesetName = tilesetName;
		TilesetX = tX;
		TilesetY = tY;
		Passable = passable;
		Depth = depth;
	}
}
