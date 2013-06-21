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
	
	public MapEditorWindow(WindowType type, Rect coords, GUI.WindowFunction renderFunction) {
		MapEditorWindow(type, coords, renderFunction, type.ToString(), type.ToString());
	}
	
	public MapEditorWindow(WindowType type, Rect coords, GUI.WindowFunction renderFunction, string displayName) {
		MapEditorWindow(type, coords, renderFunction, displayName, displayName);
	}
	
	public MapEditorWindow(WindowType type, Rect coords, GUI.WindowFunction renderFunction, string windowTitle, string buttonName) {
		Type = type;
		Coords = coords;
		Showing = false;
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
	public GameObject DefaultTilesetPrefab {get; set;}
	public GameObject SelectedTilesetPrefab {get; set;}
	
	Map map;
	
	List<MapEditorWindow> windows = new List<MapEditorWindow>();
	List<ConfigurableMapValue> configurableMapValues = new List<ConfigurableMapValue>();
	
	void Awake() {
		map = transform.GetComponent<Map>();
		
		configurableMapValues.Add(new ConfigurableMapValue("name", "Name:", map.MapName));
		configurableMapValues.Add(new ConfigurableMapValue("rows", "Rows:", map.Rows.ToString()));
		configurableMapValues.Add(new ConfigurableMapValue("columns", "Columns:", map.Columns.ToString()));
		configurableMapValues.Add(new ConfigurableMapValue("tileWidth", "Tile Width:", map.TileWidth.ToString()));
		configurableMapValues.Add(new ConfigurableMapValue("tileHeight", "Tile Height:", map.TileHeight.ToString()));
		
		windows.Add(new MapEditorWindow(MapEditorWindow.WindowType.Properties, new Rect(0, 0, 104, 200), DrawPropertyWindow));
		windows.Add(new MapEditorWindow(MapEditorWindow.WindowType.Palette, new Rect(0, 200, 300, 300), DrawPaletteWindow));
		
		if(string.IsNullOrEmpty(defaultTilesetName)) defaultTilesetName = Directory.GetFiles(string.Format("{0}/Assets/Resources/Tilesets/", Directory.GetCurrentDirectory()))[0];
		defaultTilesetName = defaultTilesetName.Split(Path.DirectorySeparatorChar).Last().Replace(".prefab", "");
		selectedTilesetPrefab = defaultTilesetPrefab = Resources.Load(string.Format("Tilesets/{0}", defaultTilesetName)) as GameObject;
		
		map.Dirty = true;
	}
	
	void OnGUI() {
		windows.EachWithIndex((window, i) => {
			if(GUILayout.Button(window.ButtonName)) {
				window.Showing = !window.Showing;
			}
			
			if(window.Showing) {
				window.Coords = GUILayout.Window(i, window.Coords, window.RenderFunction(i), window.WindowTitle);
			}
		});
	}
	
	public void DrawPalleteWindow(int id) {
	}
	
	public void DrawPropertyWindow(int id) {
		configurableMapValues.Each(mapValue => {
			GUILayout.Label(mapValue.DisplayName);
			mapValue.InputValue = GUILayout.TextField(mapValue.InputValue);
			if(mapValue.InputValue != mapValue.InputPreviousValue) {
				mapValue.InputPreviousValue = mapValue.InputValue;
				switch(mapValue.Id) {
				case "rows":
					SetInt(out map.Rows, mapValue.InputValue);
					break;
				case "columns":
					SetInt(out map.Columns, mapValue.InputValue);
					break;
				case "tileWidth":
					SetInt(out map.TileWidth, mapValue.InputValue);
					break;
				case "tileHeight":
					SetInt(out map.TileHeight, mapValue.InputValue);
					break;
				case "name":
					map.MapName = mapValue.InputValue;
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
		map.Dirty = true;
	}
}
