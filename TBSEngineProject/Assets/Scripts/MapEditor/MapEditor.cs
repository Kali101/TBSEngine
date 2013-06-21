using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GoodStuff.NaturalLanguage;
using System.Linq;
using System.IO;

[System.Serializable]
public class ConfigurableMapValue {
	public string Id { get; set; }
	public string DisplayName { get; set; }
	public string InputValue { get; set; }
	public string InputPreviousValue { get; set; }
	
	public ConfigurableMapValue(string id, string name, string defaultValue) {
		Id = id;
		DisplayName = name;
		InputPreviousValue = InputValue = defaultValue;
	}
}

public class MapEditor : MonoBehaviour {
	public string defaultTilesetName = "";
	GameObject defaultTilesetPrefab;
	GameObject selectedTilesetPrefab;
	
	string mapName = "Untitled";
	int rows = 1;
	int columns = 1;
	int tileWidth = 20;
	int tileHeight = 20;
	
	List<ConfigurableMapValue> configurableMapValues = new List<ConfigurableMapValue>();
	
	public GameObject tilePrefab;
	List<List<GameObject>> mapTiles = new List<List<GameObject>>();
	bool dirty;
	
	Rect configurationWindowCoords = new Rect(0, 0, 104, 200);
	
	void Awake() {
		configurableMapValues.Add(new ConfigurableMapValue("name", "Name:", mapName));
		configurableMapValues.Add(new ConfigurableMapValue("rows", "Rows:", rows.ToString()));
		configurableMapValues.Add(new ConfigurableMapValue("columns", "Columns:", columns.ToString()));
		configurableMapValues.Add(new ConfigurableMapValue("tileWidth", "Tile Width:", tileWidth.ToString()));
		configurableMapValues.Add(new ConfigurableMapValue("tileHeight", "Tile Height:", tileHeight.ToString()));
		
		if(string.IsNullOrEmpty(defaultTilesetName)) defaultTilesetName = Directory.GetFiles(string.Format("{0}/Assets/Resources/Tilesets/", Directory.GetCurrentDirectory()))[0];
		defaultTilesetName = defaultTilesetName.Split(Path.DirectorySeparatorChar).Last().Replace(".prefab", "");
		Debug.Log(defaultTilesetName);
		selectedTilesetPrefab = defaultTilesetPrefab = Resources.Load(string.Format("Tilesets/{0}", defaultTilesetName)) as GameObject;
		
		dirty = true;
	}
	
	void Update() {
		if(dirty) {
			RegenerateMap();
			dirty = false;
		}
	}
	
	void RegenerateMap() {
		Debug.Log("Generating map "+rows+" "+columns);
		var currentMapTiles = new List<List<GameObject>>();
		0.UpTo(rows-1, i => {
			var newRow = new List<GameObject>();
			0.UpTo(columns-1, j => {
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
				if(j >= columns) GameObject.Destroy(mapTile);
			});					
			if(i >= rows) mapRow.Each(mapTile => GameObject.Destroy(mapTile));
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
		var tilesetInfo = defaultTilesetPrefab.GetComponent<Tileset>();
		newTile.renderer.material.mainTexture = tilesetInfo.tilemapImage;
		newTile.renderer.material.SetTextureScale("_MainTex", new Vector2(1.0f/tilesetInfo.rows, 1.0f/tilesetInfo.columns));
		PositionTile(newTile);
		return newTile;
	}
	
	void PositionTile(GameObject tileObj) {
		var tile = tileObj.GetComponent<Tile>();
		tileObj.transform.localScale = new Vector3(tileWidth, 1.0f, tileHeight);
		tileObj.transform.position = new Vector3(tileObj.transform.parent.position.x + (tileWidth*tile.j) - (0.5f*columns*tileWidth), tileObj.transform.parent.position.y + (tileHeight*tile.i) - (0.5f*rows*tileHeight), tileObj.transform.position.z);
	}
	
	void OnGUI() {
		configurationWindowCoords = GUILayout.Window(0, configurationWindowCoords, DrawConfigurationWindow, "Configure Map");
	}
	
	void DrawConfigurationWindow(int id) {
		configurableMapValues.Each(mapValue => {
			GUILayout.Label(mapValue.DisplayName);
			mapValue.InputValue = GUILayout.TextField(mapValue.InputValue);
			if(mapValue.InputValue != mapValue.InputPreviousValue) {
				mapValue.InputPreviousValue = mapValue.InputValue;
				switch(mapValue.Id) {
				case "rows":
					SetInt(out rows, mapValue.InputValue);
					break;
				case "columns":
					SetInt(out columns, mapValue.InputValue);
					break;
				case "tileWidth":
					SetInt(out tileWidth, mapValue.InputValue);
					break;
				case "tileHeight":
					SetInt(out tileHeight, mapValue.InputValue);
					break;
				case "name":
					mapName = mapValue.InputValue;
					break;
				}
			}
		});
		GUI.DragWindow();
	}
	
	void SetInt(out int field, string val) {
		if(!int.TryParse(val, out field)) {
			field = 0;
		}
		dirty = true;
	}
}
