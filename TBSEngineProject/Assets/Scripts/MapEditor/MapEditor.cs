using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GoodStuff.NaturalLanguage;
using System.Linq;
using System.IO;

public class MapEditorWindow {
	public enum WindowType {
		Properties,
		Palette,
	}
	public WindowType Type { get; set; }
	public Rect Coords { get; set; }
	public bool Showing { get; set; }
	public string WindowTitle { get; set; }
	public string ButtonName { get; set; }
	public GUI.WindowFunction RenderFunction;
	
	public MapEditorWindow(WindowType type, Rect coords, GUI.WindowFunction renderFunction, string windowTitle, string buttonName) {
		Type = type;
		Coords = coords;
		Showing = true;
		RenderFunction = renderFunction;
		WindowTitle = windowTitle;
		ButtonName = buttonName;
	}
}

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
	public Tileset SelectedTileset {get; set;}
	public int SelectedTilesetXOffset {
		get {
			var gridBox = tilesetDisplay.GetComponent<GridBoxSelector>();
			if(gridBox == null) return 0;
			return map.TileHeight * gridBox.GridXSelected;
		}
	} 
	public int SelectedTilesetYOffset {
		get {
			var gridBox = tilesetDisplay.GetComponent<GridBoxSelector>();
			if(gridBox == null) return 0;
			return map.TileWidth * gridBox.GridYSelected;		
		}
	} 
	
	Vector2 scrollPosition = new Vector2();
	string selectedTilesetName = "";
	public Map map;
	
	List<string> allTilesets = new List<string>();
	public GameObject tilesetDisplay;
	
	List<MapEditorWindow> windows = new List<MapEditorWindow>();
	List<ConfigurableMapValue> configurableMapValues = new List<ConfigurableMapValue>();
	
	void Awake() {
		configurableMapValues.Add(new ConfigurableMapValue("name", "Name:", map.MapName));
		configurableMapValues.Add(new ConfigurableMapValue("rows", "Rows:", map.Rows.ToString()));
		configurableMapValues.Add(new ConfigurableMapValue("columns", "Columns:", map.Columns.ToString()));
		configurableMapValues.Add(new ConfigurableMapValue("tileWidth", "Tile Width:", map.TileWidth.ToString()));
		configurableMapValues.Add(new ConfigurableMapValue("tileHeight", "Tile Height:", map.TileHeight.ToString()));
		
		windows.Add(new MapEditorWindow(MapEditorWindow.WindowType.Properties, new Rect(10, 50, 104, 200), DrawPropertyWindow, "Properties", "Properties"));
		windows.Add(new MapEditorWindow(MapEditorWindow.WindowType.Palette, new Rect(0, 340, 200, 200), DrawPaletteWindow, "Palette", "Palette"));
		
		allTilesets = Directory.GetFiles(string.Format("{0}/Assets/Resources/Tilesets/", Directory.GetCurrentDirectory()))
			.Where(p => !p.EndsWith(".meta"))
			.Select(t => t.Split(Path.DirectorySeparatorChar).Last().Replace(".prefab", "")).ToList();
		
		selectedTilesetName = string.IsNullOrEmpty(defaultTilesetName) ? allTilesets[0] : defaultTilesetName;
		LoadTileset();
	}
	
	void LoadTileset() {
		SelectedTileset = (Resources.Load(string.Format("Tilesets/{0}", selectedTilesetName)) as GameObject).GetComponent<Tileset>();		
		var gridBox = tilesetDisplay.GetComponent<GridBoxSelector>();
		if(gridBox == null) gridBox = tilesetDisplay.AddComponent<GridBoxSelector>();
		gridBox.TileWidth = map.TileWidth;
		gridBox.TileHeight = map.TileHeight;
		
		gridBox.GridXSelected = gridBox.GridYSelected = 0;
		
		tilesetDisplay.renderer.material.mainTexture = SelectedTileset.tilemapImage;
		var gridTexture = new Texture2D(SelectedTileset.tilemapImage.width, SelectedTileset.tilemapImage.height);
		0.UpTo(gridTexture.width, i => {
			0.UpTo(gridTexture.height, j => {
				if(i % map.TileWidth == 0 || j % map.TileWidth == 0) {
					gridTexture.SetPixel(i, j, Color.black);
				} else {
					gridTexture.SetPixel(i, j, Color.clear);
				}
			});
		});
		gridTexture.Apply();
		tilesetDisplay.transform.FindChild("GridOverlay").renderer.material.mainTexture = gridTexture;
	}
	
	void OnGUI() {
		windows.EachWithIndex((window, i) => {
			if(GUILayout.Button(window.ButtonName)) {
				window.Showing = !window.Showing;
			}
			
			if(window.Showing) {
				window.Coords = GUILayout.Window(i, window.Coords, window.RenderFunction, window.WindowTitle);
			}
		});
	}
	
	public void DrawPaletteWindow(int id) {
		GUILayout.BeginHorizontal();
		GUILayout.Label(string.Format("Current Tileset: {0}", selectedTilesetName));
		if(GUILayout.Button(tilesetDisplay.activeSelf ? "Hide" : "Show")) {
			tilesetDisplay.SetActive(!tilesetDisplay.activeSelf);
		}
		GUILayout.EndHorizontal();
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		allTilesets.Each(tileset => {
			if(GUILayout.Button(tileset)) {
				selectedTilesetName = tileset;
				LoadTileset();
			}
		});
		GUILayout.EndScrollView();
		
		GUI.DragWindow();
	}
	
	public void DrawPropertyWindow(int id) {
		configurableMapValues.Each(mapValue => {
			GUILayout.Label(mapValue.DisplayName);
			mapValue.InputValue = GUILayout.TextField(mapValue.InputValue);
			if(mapValue.InputValue != mapValue.InputPreviousValue) {
				mapValue.InputPreviousValue = mapValue.InputValue;
				var result = -1;
				switch(mapValue.Id) {
				case "rows":
					map.Rows = int.TryParse(mapValue.InputValue, out result) ? result : 0;
					break;
				case "columns":
					map.Columns = int.TryParse(mapValue.InputValue, out result) ? result : 0;
					break;
				case "tileWidth":
					map.TileWidth = int.TryParse(mapValue.InputValue, out result) ? result : 0;
					break;
				case "tileHeight":
					map.TileHeight = int.TryParse(mapValue.InputValue, out result) ? result : 0;
					break;
				case "name":
					map.MapName = mapValue.InputValue;
					break;
				}
				map.Dirty = true;
			}
		});
		GUI.DragWindow();
	}
}
