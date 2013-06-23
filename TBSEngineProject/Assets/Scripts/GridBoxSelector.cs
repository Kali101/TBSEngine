using UnityEngine;
using System.Collections;
using GoodStuff.NaturalLanguage;

public class GridBoxSelector : MonoBehaviour {
	public int Rows {
		get {
			return rows;
		}
		set {
			rows = value;
			ResizeSelectionBox();
		}
	}
	public int Columns {
		get {
			return columns;
		}
		set {
			columns = value;
			ResizeSelectionBox();
		}
	}
	
	public int GridXSelected {get; set;}
	public int GridYSelected {get; set;}
	public int GridXHover {get; set;}
	public int GridYHover {get; set;}
		
	public bool SendMessageWhenClicked = true;
	public string MessageId;
	
	public GameObject SelectionBoxPrefab;
	
	GameObject selectionBox;
	
	int rows = -1;
	int columns = -1;
	
	void Awake() {
		if(SendMessageWhenClicked && string.IsNullOrEmpty(MessageId)) MessageId = gameObject.name;
	}
	
	void CreateSelectionBox() {
		selectionBox = GameObject.Instantiate(SelectionBoxPrefab) as GameObject;
		selectionBox.gameObject.name = string.Format("SelectionBox{0}", gameObject.name);
		selectionBox.transform.parent = transform;
		selectionBox.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
	}
	
	void ResizeSelectionBox() {
		if(selectionBox == null) CreateSelectionBox();
		selectionBox.transform.localScale = new Vector3(1.0f / Columns, selectionBox.transform.localScale.y, 1.0f / Rows);
	}
	
	void OnMouseExit() {
		selectionBox.renderer.enabled = false;
	}
	
	void OnMouseEnter() {
		selectionBox.renderer.enabled = true;
	}
	
	void OnMouseOver() {
		RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
		GridXHover = Mathf.FloorToInt(hit.textureCoord.x * Columns);
		GridYHover = Mathf.FloorToInt(hit.textureCoord.y * Rows);
		
		var newX = (float)GridXHover.MapToRange(0, Columns, collider.bounds.min.x, collider.bounds.max.x) + selectionBox.renderer.bounds.extents.x;
		var newZ = (float)GridYHover.MapToRange(0, Rows, collider.bounds.min.z, collider.bounds.max.z) + selectionBox.renderer.bounds.extents.z;
		selectionBox.transform.position = new Vector3(newX, transform.position.y + 0.1f, newZ);
	}
	
	void OnMouseDown() {
		if(Rows < 0 || Columns < 0) { 
			Debug.LogError(string.Format("ERROR: Grid on {0} has been clicked before row or column properties are set.", gameObject.name));
			return;
		}
		
		GridXSelected = GridXHover;
		GridYSelected = GridYHover;
		
		if(SendMessageWhenClicked) EventRouter.Publish(MessageId, GameEvent.Input.GridClicked, GridXSelected, GridYSelected);
	}
	
}
