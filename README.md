# Asset Organizer Tool  
*A modern Unity Editor tool for organizing ScriptableObject items, validating data, and speeding up production workflows. Created using Editor Utility and UI Toolkit*

---

## Preview  


**Main Window:**  

<img width="1261" height="522" alt="image" src="https://github.com/user-attachments/assets/cd1b6c66-47e7-4b47-9df3-7ba1bff7ed49" />

---

# Architecture (MVVM)

### **Model**  
- ScriptableObject (`GameItemConfig`)

### **ViewModel**  
- Filtering logic  
- Searching  
- Observable properties  
- Scanner logic  

### **View**  
- Unity UI Toolkit window  
- UXML + USS  
- Context menus  
- Custom inspector

---
#  Features

## Intelligent Asset Scanner
- Recursively scans selected folders  
- Detects all `GameItemConfig` assets  
- Async workflow 
- Smooth progress bar  
- Overlay during scan

**Scanning UI:**  
<img width="1266" height="521" alt="image" src="https://github.com/user-attachments/assets/6bee1bc2-d0c7-42ba-89ca-1225ea7ce961" />


---

## Advanced List View
- Custom row templates (icon + name + category + price)  
- Validation badges (error, warning)  
- Dynamic category filter  
- Smart search  
- Auto-refresh on asset changes  
- Context menu:
  - Rename
  - Duplicate
  - Reveal in Finder
  - Ping
  - Move To…
  - Delete

**List View:**  
<img width="361" height="365" alt="image" src="https://github.com/user-attachments/assets/269770b7-abb6-4095-814f-fbbb2b0ecadb" />


---

## Custom Inspector for GameItemConfig
- Clean UIToolkit-based inspector  
- Live-updating validation badges  
- Auto-bound PropertyFields  
- Icon preview  
- “Open in Asset Organizer” quick-access button

**Custom Inspector:**  
<img width="611" height="565" alt="image" src="https://github.com/user-attachments/assets/d532e347-27c5-48f5-a5d5-66bb6e1fa438" />


---

## Batch Operations
### Batch Rename
- Add prefix  
- Add suffix  
- Undo-supported  

### Batch Category
- Assign new category to all filtered items  

**Batch Tools:**  
<img width="758" height="662" alt="image" src="https://github.com/user-attachments/assets/6fe0c3a2-81f1-4f15-ae7b-eee3a09f26df" />
<img width="549" height="389" alt="image" src="https://github.com/user-attachments/assets/628cbbd7-0801-43d2-8824-26909d44d134" />



---

## Keyboard Shortcuts

| Shortcut | Action |
|---------|--------|
| **Ctrl + F** | Focus search field |
| **Ctrl + N** | Create new item |
| **Ctrl + R** | Scan assets |
| **Ctrl + Shift + R** | Batch rename |
| **Ctrl + Shift + C** | Batch change category |
| **F2** | Rename selected |
| **Delete** | Delete selected |

# Validation System  

The tool uses `GameItemValidator` to analyze each item:

- Missing icon → **Warning**  
- Empty display name → **Error**  
- File name mismatch → **Warning**  
- Negative price → **Error**  
- Any custom rule you add  

Badges update **instantly** when fields change.
