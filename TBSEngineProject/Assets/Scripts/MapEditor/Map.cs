using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GoodStuff.NaturalLanguage;
using System.Linq;

public class Map : MonoBehaviour {
	public GameObject tilePrefab;
	List<List<GameObject>> mapTiles = new List<List<GameObject>>();
	
	public string MapName { get; set; }
	public int Rows { get; set; }
	public int Columns { get; set; }
	public int TileWidth { get; set; }
	public int TileHeight { get; set; }
	
	public bool Dirty { get; set; } 
	
	MapEditor editor;
	
	void Awake() {
		editor = GetComponent<MapEditor>();
	}
	
	void Update() {
		if(Dirty) {
			RegenerateMap();
			Dirty = false;
		}
	}
	
	void RegenerateMap() {
		Debug.Log("Generating map "+Rows+" "+Columns);
		var currentMapTiles = new List<List<GameObject>>();
		0.UpTo(Rows-1, i => {
			var newRow = new List<GameObject>();
			0.UpTo(Columns-1, j => {
				if(mapTiles.Count > i && mapTiles[i].Count > j) { 
					newRow.Add(mapTiles[i][j]);
					PositionTile(mapTiles[i][j]);
				}
				else newRow.Add(CreateNewTile(i, j));				
			});
			
			currentMapTiles.Add(newRow);
		});
		mapTiles.EachWithIndex((mapRow, i) => {
			mapRow.EachWithIndex((mapTile, j) => {
				if(j >= Columns) GameObject.Destroy(mapTile);
			});					
			if(i >= Rows) mapRow.Each(mapTile => GameObject.Destroy(mapTile));
		});
		mapTiles = currentMapTiles;
	}
	
	GameObject CreateNewTile(int i, int j) {
		var newTile = GameObject.Instantiate(tilePrefab) as GameObject;
		newTile.transform.parent = this.transform;
		newTile.name = string.Format("[{0}][{1}]", i, j);
		var tile = newTile.GetComponent<Tile>();
		tile.i = i;
		tile.j = j;
		var tilesetInfo = editor.DefaultTilesetPrefab.GetComponent<Tileset>();
		newTile.renderer.material.mainTexture = tilesetInfo.tilemapImage;
		newTile.renderer.material.SetTextureScale("_MainTex", new Vector2(1.0f/tilesetInfo.columns, 1.0f/tilesetInfo.rows));
		PositionTile(newTile);
		return newTile;
	}
	
	void PositionTile(GameObject tileObj) {
		var tile = tileObj.GetComponent<Tile>();
		tileObj.transform.localScale = new Vector3(TileWidth, 1.0f, TileHeight);
		tileObj.transform.position = new Vector3(tileObj.transform.parent.position.x + (TileWidth*tile.j) - (0.5f*Columns*TileWidth), tileObj.transform.parent.position.y + (TileHeight*tile.i) - (0.5f*Rows*TileHeight), tileObj.transform.position.z);
	}
	
}
