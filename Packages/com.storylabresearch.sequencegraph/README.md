# StoryLab Sequence Graph

A node-graph driven sequencing system for Unity, built on [xNode](https://github.com/Siccity/xNode). Author ordered sequences of **control**, **condition**, and **action** nodes in a visual graph, then drive them at runtime with a `SequenceGraphParser` component.

This is the **XR-agnostic core**. For XR Interaction Toolkit nodes (fade, teleport, scene load), add [com.storylabresearch.sequencegraph.xri](../com.storylabresearch.sequencegraph.xri).

## ⚠️ Scene only — a Sequence Graph cannot live in a prefab

A `SequenceGraph` is a `ScriptableObject` held by reference from the parser. Unity
serialises a referenced `ScriptableObject` instance inline into a **scene** file, but a
**prefab asset cannot hold one**. A graph authored in Prefab Mode looks like it works and
is then discarded, without warning, the moment the prefab is closed.

**Put `SequenceGraphParser` on a plain GameObject at the top level of a scene.** For
sequencing that has to ship inside a prefab, use **Unity Timeline** instead.

Since 0.2.0 the package makes this loud rather than silent:

- The parser inspector shows a red error and hides the graph controls whenever the
  parser is part of a prefab asset — either opened in Prefab Mode or selected in the
  Project window.
- On a **prefab instance** in a scene, the inspector shows a milder warning: the graph
  is held as a scene override, so it does work, but it can never be applied back to the
  prefab asset and "Apply All" will discard it.
- On entering play mode with no graph assigned, the parser logs an error naming the
  prefab cause and stays inert, rather than throwing a `NullReferenceException`.

### Why this cannot simply be fixed by saving the graph to disk

Moving the graph into a `.asset` file would make it prefab-safe, but Unity does not
permit an asset to reference a scene object — such references serialise as null. Three
built-in nodes depend on exactly that: `DestroyObjectsNode.objects`,
`UnityEventNode.OnTriggered`, and `UnityEventConditionNode._behaviour`.

Timeline is not exempt from this rule; it works around it. A `TimelineAsset` holds no
scene references at all — they live in a binding table on the `PlayableDirector`
component in the scene or prefab, resolved at runtime. That indirection is why Timeline
uses signals and bindings rather than direct method calls. Adopting the same split here
would mean reworking every scene-referencing node onto `ExposedReference<T>` (and
replacing `UnityEventNode` with a signal the parser owns). That is a deliberate
non-goal for now: the recommendation is scene-level Sequence Graph, Timeline inside
prefabs.

## Concepts

- **Control nodes** form the spine of the sequence — execution walks from the `EntryNode` along `Output` connections, one node at a time.
- A control node's `EvaluateNode()` decides whether the sequence may advance past it (e.g. `WaitForConditionNode` blocks until its condition is true).
- **Action nodes** hang off a control node's `Trigger` port and fire (`OnTrigger()`) when that node is entered.
- **Condition nodes** feed boolean results into `WaitFor...` control nodes and can be combined (All / Any / NotAll / None).

## Built-in nodes

- **Control:** Entry, Wait For Condition, Wait For Conditions
- **Condition:** Time, Unity Event, Combine Conditions
- **Action:** Debug Log, Destroy Objects, Fog, Skybox Material, Unity Event

## Usage

1. Add a `SequenceGraphParser` to a GameObject **in a scene** (menu: **StoryLabResearch ▸ Sequence Graph**). Do not do this inside a prefab — see the warning above.
2. In its inspector, click **New graph**, then **Open graph** to edit nodes.
3. Build your sequence from the Entry node outward.
4. Set `ParseOnEnable` to start automatically, or call `Play()` / `Pause()` / `Stop()`.
5. Save the scene. The graph is stored in the scene file alongside the parser.

**Remove graph** asks for confirmation before deleting. Because the graph lives in the
scene rather than in an asset file, deleting it cannot be recovered afterwards.

## Dependencies

- `com.github.siccity.xnode` — the node-graph framework
- `com.github.siccity.xnodegroups` — node grouping (the editor uses `XNode.NodeGroups.NodeGroup`)

Both are declared in `package.json` and pulled as git packages, but because UPM does
not resolve git dependencies transitively, a consuming project must add both git URLs
to its own manifest. See the repository root `README.md`.

## Notes for maintainers

- The runtime assembly (`StoryLabResearch.SequenceGraph`) references **only** `XNode`.
  All editor concerns live in `StoryLabResearch.SequenceGraph.Editor`. To keep that
  boundary clean, the menu-path helper in `ConditionNode` inlines xNode's
  `NodeEditorUtilities.NodeDefaultPath` rather than referencing the editor assembly.
- Editor menu labels for nodes are derived from the **namespace string**. If the
  namespace changes, node-menu paths change with it.
- Prefab detection in `SequenceGraphParserEditor` needs **two** checks:
  `PrefabUtility.IsPartOfPrefabAsset` covers a prefab selected in the Project window,
  but Prefab Mode contents live in a preview scene where it reports `false`, so
  `PrefabStageUtility.GetPrefabStage` is also required. Prefab Mode is the case that
  actually loses work, so do not drop it.
- **A hand-rolled `OnBodyGUI` must write its fields through `Undo.RecordObject`.** Graph
  nodes are unowned `ScriptableObject`s serialised inline into the scene: they are
  neither persistent assets nor scene objects, so `EditorUtility.SetDirty` has no effect
  on them and a plain field write never marks the scene dirty. Unity then never offers
  to save and the edit is silently discarded on reload. Registering an undo is the only
  thing that dirties the owning scene. Nodes drawn by xNode's default `NodeEditor.OnBodyGUI`
  get this for free via `serializedObject.ApplyModifiedProperties()`, which is why
  `UnityEventNode.OnTriggered` always persisted while `UnityEventConditionNode._behaviour`
  did not.
- `WaitForConditionNode._internalConditionNode` is a `ScriptableObject` created with
  `CreateInstance` that is never added to `graph.nodes`. It survives only because scene
  files serialise unowned `ScriptableObject` instances inline. Anything that moves a
  graph into an `.asset` file must adopt these via `AssetDatabase.AddObjectToAsset` or
  every Wait For Condition node will silently lose its condition on the next reload.
