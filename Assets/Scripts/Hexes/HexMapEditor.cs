using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
public class HexMapEditor : MonoBehaviour{


    public Material terrainMaterial;
    int activeElevation;

    bool applyElevation = false;
    int activeWaterLevel;
    bool applyWaterLevel = false;
    int activeUrbanLevel;
    bool applyUrbanLevel = false;
    int activeFarmLevel;
    bool applyFarmLevel = false;
    int activePlantLevel;
    bool applyPlantLevel = false;
    int activeSpecialIndex;
    bool applySpecialIndex = false;
    int activeTerrainTypeIndex;
    public HexGrid hexGrid;
    int brushSize = 0;
    enum OptionalToggle
    {
        Ignore, Yes, No
    }

    OptionalToggle riverMode, roadMode, walledMode;

    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;
    public void SetElevation(float elevation) {
        activeElevation = (int)elevation;
    }
    public void SetTerrainTypeIndex(int index) {
        activeTerrainTypeIndex = index;
    }
    public void SetApplyUrbanLevel(bool toggle) {
        applyUrbanLevel = toggle;
    }
    public void SetApplyFarmLevel(bool toggle) {
        applyFarmLevel = toggle;
    }
    public void SetApplyPlantLevel(bool toggle) {
        applyPlantLevel = toggle;
    }
    public void SetApplySpecialIndex(bool toggle) {
        applySpecialIndex = toggle;
    }
    public void SetSpecialIndex(float index) {
        activeSpecialIndex = (int)index;
    }
    public void SetUrbanLevel(float urbanLevel) {
        activeUrbanLevel = (int)urbanLevel;
    }
    public void SetFarmLevel(float farmLevel) {
        activeFarmLevel = (int)farmLevel;
    }
    public void SetPlantLevel(float plantLevel) {
        activePlantLevel = (int)plantLevel;
    }
    public void SetEditMode(bool toggle) {
        enabled = toggle;
    }



    public void ShowUI(bool visible) {
        hexGrid.ShowUI(visible);
    }

    void Awake() {
        terrainMaterial.DisableKeyword("GRID_ON");
        Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
        SetEditMode(true);
    }
    void Update() {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButton(0)) {
                HandleInput();
                return;
            }
            if (Input.GetKeyDown(KeyCode.U)) {
                if (Input.GetKey(KeyCode.LeftShift)) {
                    DestroyUnit();
                }
                else {
                    CreateUnit();
                }
            }
        }
        previousCell = null;
    }


    void HandleInput() {
        
        HexCell currentCell = GetCellUnderCursor();

        if (currentCell) {
            if (previousCell && previousCell != currentCell) {
                ValidateDrag(currentCell);
            }
            else {
                isDrag = false;
            }

            EditCells(currentCell);
            previousCell = currentCell;
        }
        else {
            previousCell = null;
        }
    }

    void CreateUnit() {
        HexCell cell = GetCellUnderCursor();
        if (cell && !cell.Unit) {
            hexGrid.AddUnit(
                            Instantiate(HexUnit.unitPrefab), cell, Random.Range(0f, 360f)
                        );
        }
    }
    void DestroyUnit() {
        HexCell cell = GetCellUnderCursor();
        if (cell && cell.Unit) {
            hexGrid.RemoveUnit(cell.Unit);
        }
    }

    void EditCells(HexCell center) {
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;
        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
            for (int x = centerX - r; x <= centerX + brushSize; x++) {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
            for (int x = centerX - brushSize; x <= centerX + r; x++) {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

    }
    void EditCell(HexCell cell) {
        if (cell) {
            //Debug.Log("Editing cell at co-ordinates " + cell.coordinates.ToString());
            if (activeTerrainTypeIndex >= 0) {
                cell.TerrainTypeIndex = activeTerrainTypeIndex;
            }
            if (applyElevation) {
                cell.Elevation = activeElevation;
            }
            if (applyWaterLevel) {
                cell.WaterLevel = activeWaterLevel;
            }
            if (riverMode == OptionalToggle.No) {
                cell.RemoveRiver();
            }
            if (roadMode == OptionalToggle.No) {
                cell.RemoveRoads();
            }
            if (walledMode != OptionalToggle.Ignore) {
                cell.Walled = walledMode == OptionalToggle.Yes;
            }
            if (applyUrbanLevel) {
                cell.UrbanLevel = activeUrbanLevel;
            }
            if (applyFarmLevel) {
                cell.FarmLevel = activeFarmLevel;
            }
            if (applyPlantLevel) {
                cell.PlantLevel = activePlantLevel;
            }
            if (applySpecialIndex) {
                cell.SpecialIndex = activeSpecialIndex;
            }
            if (isDrag) {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell) {
                    if (riverMode == OptionalToggle.Yes) {
                        otherCell.SetOutgoingRiver(dragDirection);
                    }
                    if (roadMode == OptionalToggle.Yes) {
                        otherCell.AddRoad(dragDirection);
                    }
                }
            }
        }
    }


    public void SetRiverMode(int mode) {
        riverMode = (OptionalToggle)mode;
    }
    public void SetRoadMode(int mode) {
        roadMode = (OptionalToggle)mode;
    }
    public void SetWalledMode(int mode) {
        walledMode = (OptionalToggle)mode;
    }
    public void SetApplyElevation(bool toggle) {
        applyElevation = toggle;
    }
    public void SetApplyWaterLevel(bool toggle) {
        applyWaterLevel = toggle;
    }
    public void SetBrushSize(float size) {
        brushSize = (int)size;
    }
    public void SetWaterLevel(float level) {
        activeWaterLevel = (int)level;
    }
    public void ShowGrid(bool visible) {
        if (visible) {
            terrainMaterial.EnableKeyword("GRID_ON");
        }
        else {
            terrainMaterial.DisableKeyword("GRID_ON");
        }
    }



   


    HexCell GetCellUnderCursor() {
        return hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }
    void ValidateDrag(HexCell currentCell) {
        for (dragDirection = HexDirection.NE;
            dragDirection <= HexDirection.NW;
            dragDirection++
            ) {
            if (previousCell.GetNeighbor(dragDirection) == currentCell) {
                isDrag = true;
                return;
            }
        }
        isDrag = false;

    }
}
