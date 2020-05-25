# Monbetsu #
Monbetsu is a classifier for edges/subgraphs in a DAG.
It makes hierarchical groups so that thier components (edge or subgraph) are connected with no branchs,
and labels them respectively.
The components of each groups share the same label.

For example, Monbetsu labels the edges/subgraphs in the below DAG  
![original graph](./docs/images/example01-original-graph.png)   
as the following:  
![labeled graph](./docs/images/example01-labeled-graph.png)   

The targets of labeling are as follows:

- Edge: each edges of the graph.
- Subgraphs: they have single start node, single end node, and multiple labeled edges/sub-subgraphs.
  - Series: their components share a label and sequentially connected. They are greedly grouped so that any of the componsents are not Series.  
    ![series](./docs/images/subgraph-series.png)   
  - Parallel: the labels of their components share a pair of start/end node. They are greedly grouped so that any of the componsents are not Parallel.  
    ![parallel](./docs/images/subgraph-parallel.png)   
  - Knot: otherwise.  
    ![knot](./docs/images/subgraph-knot.png)   

  The other kinds of subgraph which have multiple start/end nodes are not labeled.

The labeling is hierarchical and bottom-up (i.e. edges -> smaller subgraphs -> larger subgraphs).  
The following animation illustrates the steps of labeling.  
![steps](./docs/images/example01-steps.gif)   


## Installation ##
Copy [MonbetsuClassifier.cs](./Monbetsu/MonbetsuClassifier.cs) to your project. (Monbetsu is not in NuGet.)

#### Prerequisites ####
- C# 8.0 or above
- one of the following
  - .NET Standard  2.0 or above
  - .NET Core 2.0 or above
  - .NET Framework 4.7.1 or above
  
If your project uses the older version, some modifications (e.g. adding reference to System.ValueTuple) would be required.

## Usage ##

Call one of ``Classify`` methods declared in class ``MonbetsuClassifier``, ``MonbetsuClassifier`3``, or ``MonbetsuClassifier`4``.
The arguments of the overloads are combinations of:

- with or without Graph object
- interface-base or delegate-base


#### Example 1 ####
```csharp ../Monbetsu.Test/Docs.cs
namespace Monbetsu.Tutorials.Tutorial1
{
}
```

#### Example 2 ####
```csharp ../Monbetsu.Test/Docs.cs
namespace Monbetsu.Tutorials.Tutorial2
{
}

```

## Demo ##
Online demo page implemented by Blazor is [here](https://takeisit.github.io/Monbetsu/).
