# sequence-graph

Development monorepo for the **StoryLab Sequence Graph** Unity packages. This repository is itself a minimal Unity 6 project so the node-graph editor can be opened and tested in place; the shippable artifacts are the packages under [`Packages/`](Packages/).

## Packages

| Package | Folder | Description |
|---|---|---|
| `com.storylabresearch.sequencegraph` | [`Packages/com.storylabresearch.sequencegraph`](Packages/com.storylabresearch.sequencegraph) | XR-agnostic core. Sequencing system built on xNode. |
| `com.storylabresearch.sequencegraph.xri` | [`Packages/com.storylabresearch.sequencegraph.xri`](Packages/com.storylabresearch.sequencegraph.xri) | XR Interaction Toolkit nodes. Depends on the core. **(stub — nodes not yet ported)** |

## Consuming these packages from another project

Packages are distributed by git URL using UPM's `?path=` subfolder syntax. Add the
relevant entries to the consumer project's `Packages/manifest.json`.

**Core only:**

```jsonc
{
  "dependencies": {
    "com.github.siccity.xnode": "https://github.com/Siccity/xNode.git",
    "com.github.siccity.xnodegroups": "https://github.com/StoryLab-Research-Institute/RenamablexNodeGroups.git",
    "com.storylabresearch.sequencegraph": "https://github.com/StoryLab-Research-Institute/sequence-graph.git?path=/Packages/com.storylabresearch.sequencegraph"
  }
}
```

> **Why xNode *and* xNodeGroups must be listed explicitly:** UPM does not resolve
> git-URL dependencies transitively. The core package declares both in its own
> `package.json`, but a consumer still has to add each git URL by hand — declaring the
> core alone will not pull them in. The same applies to the XRI package below.

**Core + XRI nodes:**

```jsonc
{
  "dependencies": {
    "com.github.siccity.xnode": "https://github.com/Siccity/xNode.git",
    "com.github.siccity.xnodegroups": "https://github.com/StoryLab-Research-Institute/RenamablexNodeGroups.git",
    "com.storylabresearch.sequencegraph": "https://github.com/StoryLab-Research-Institute/sequence-graph.git?path=/Packages/com.storylabresearch.sequencegraph",
    "com.storylabresearch.sequencegraph.xri": "https://github.com/StoryLab-Research-Institute/sequence-graph.git?path=/Packages/com.storylabresearch.sequencegraph.xri",
    "com.unity.xr.interaction.toolkit": "3.0.8"
  }
}
```

### Pinning a version

Append a git tag (or branch/commit) with `#`:

```
https://github.com/StoryLab-Research-Institute/sequence-graph.git?path=/Packages/com.storylabresearch.sequencegraph#v0.2.0
```

Because both packages share this repo, a tag versions **both** at once — fine while
they release in lockstep.

## Developing

Open this repository's root folder in Unity 6 (via Unity Hub). The two packages are
discovered automatically as embedded packages under `Packages/`. Edit a sequence via a
`SequenceGraphParser` component (**StoryLabResearch ▸ Sequence Graph** menu). Test
scenes live under [`Assets/`](Assets/).

> **A Sequence Graph must live in a scene, not in a prefab.** The graph is a
> `ScriptableObject` serialised inline into the scene file; prefab assets cannot hold
> one, so a graph authored in Prefab Mode is discarded when the prefab closes. Since
> 0.2.0 the parser inspector blocks this with a red error and the parser logs an error
> on play rather than failing silently. Use **Unity Timeline** for sequencing that has
> to ship inside a prefab, and see the [core package
> README](Packages/com.storylabresearch.sequencegraph) for why saving the graph to disk
> is not a workaround.

## Provenance

Extracted from the `StoryLabResearch` folder of the `ia_p1_primary` project. The
original SequenceGraph targeted UltimateXR; this repo drops UltimateXR in favour of an
XR-agnostic core plus an XRI add-on. Namespace changed from
`StoryLabResearch.Experimental.SequenceGraph` to `StoryLabResearch.SequenceGraph`.
