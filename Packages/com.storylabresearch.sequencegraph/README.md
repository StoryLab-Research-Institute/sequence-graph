# StoryLab Sequence Graph

A node-graph driven sequencing system for Unity, built on [xNode](https://github.com/Siccity/xNode). Author ordered sequences of **control**, **condition**, and **action** nodes in a visual graph, then drive them at runtime with a `SequenceGraphParser` component.

This is the **XR-agnostic core**. For XR Interaction Toolkit nodes (fade, teleport, scene load), add [com.storylabresearch.sequencegraph.xri](../com.storylabresearch.sequencegraph.xri).

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

1. Add a `SequenceGraphParser` to a GameObject (menu: **StoryLabResearch ▸ Sequence Graph**).
2. In its inspector, click **New graph**, then **Open graph** to edit nodes.
3. Build your sequence from the Entry node outward.
4. Set `ParseOnEnable` to start automatically, or call `Play()` / `Pause()` / `Stop()`.

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
