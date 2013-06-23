using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GoodStuff.NaturalLanguage;
using System.Linq;

public class Map : MonoBehaviour {
	List<List<TileInfo>> mapTiles = new List<List<TileInfo>>();
	
	public string MapName { get; set; }
	public int Rows { get; set; }
	public int Columns { get; set; }
	public int TileWidth { get; set; }
	public int TileHeight { get; set; }
	
	public bool Dirty { get; set; } 
	
	public MapEditor editor;
	
	void Awake() {
		MapName = "Untitled";
		Rows = 3;
		Columns = 3;
		TileWidth = 20;
		TileHeight = 20;
		
		Dirty = true;
		EventRouter.Subscribe("Map", GameEvent.Input.GridClicked, OnMapClicked);
	}
	
	void OnMapClicked(EventRouter.Event evt) {
		PaintTile(evt.GetData<int>(0), evt.GetData<int>(1));
	}
	
	void Update() {
		if(Dirty) {
			RegenerateMap();
			Dirty = false;
		}
	}
	
	void RegenerateMap() {
		var newMapWidth = Columns*TileWidth;
		var newMapHeight = Rows*TileHeight;
		transform.localScale = new Vector3(newMapWidth, 1, newMapHeight);
		
		renderer.material.mainTexture = new Texture2D(newMapWidth, newMapHeight); 
		
		var currentMapTiles = new List<List<TileInfo>>();
		0.UpTo(Rows-1, i => {
			var newRow = new List<TileInfo>();
			0.UpTo(Columns-1, j => {
				if(mapTiles.Count > i && mapTiles[i].Count > j) { 
					newRow.Add(mapTiles[i][j]);
					PaintTile(i, j);
				}
				else newRow.Add(CreateNewTile(i, j));				
			});
			
			currentMapTiles.Add(newRow);
		});
		mapTiles = currentMapTiles;
		
		transform.GetComponent<GridBoxSelector>().TileWidth = TileWidth;
		transform.GetComponent<GridBoxSelector>().TileHeight = TileHeight;
	}
	
	TileInfo CreateNewTile(int i, int j) {
		PaintTile(i, j);		
		return new TileInfo(i, j, editor.SelectedTileset.tilemapName, editor.SelectedTilesetXOffset, editor.SelectedTilesetYOffset);
	}
	
	void PaintTile(int i, int j) {
		Debug.Log (editor.SelectedTilesetXOffset+" "+editor.SelectedTilesetYOffset);
		var tilesetPixels = editor.SelectedTileset.tilemapImage.GetPixels(editor.SelectedTilesetXOffset, editor.SelectedTilesetYOffset, TileWidth, TileHeight);

		var mainTex = transform.renderer.material.GetTexture("_MainTex") as Texture2D;
		mainTex.SetPixels(i*TileWidth, j*TileHeight, TileWidth, TileHeight, tilesetPixels);
		mainTex.Apply();
		renderer.material.SetTexture("_MainTex", mainTex);
		
	}
}
