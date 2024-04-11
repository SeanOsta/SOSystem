Scriptable Object system that helps create cleaner architectures in projects. Currently only tested with Unity Version 2022.3.13f1

Features
1. Scriptable object data
2. Scriptable object lists
3. Scriptable object pools
4. Scriptable object events
5. Optional logging of data changes and events for debugging
6. Editor tool to search for scriptable object data within project assets, scenes, and prefabs
7. Auto generation of scripts needed for new scriptable object data types


Examples
1. To test logging start the "LoggingExampleScene", click on the "event_Example" asset. Use the editor to invoke the event which will show logs for the events and data changed.
   Note: events only log if the "DEBUG_EVENTS" scripting define symbol is set in the player. Debugging data change is only available in editor and must be enabled by setting the "Debug Stack Trace On Value Change" parameter to true

2. To test the reference finder right click on the asset named "RightClickAndChoose - Search For Scriptable Data Asset" and click "Search For Scriptable Data Asset" in the pop up context menu
   
3. To test the object pool data start scene "PoolExampleScene" and change data in the "ObjectPoolDataExample/Data" folder 
