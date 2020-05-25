window.demo = {
    draw: function (container, graph) {
        const nodes = new vis.DataSet(graph.nodes);
        const edges = new vis.DataSet(graph.edges);

        const data = {
            nodes: nodes,
            edges: edges
        };
        const options = {
            
        };
        new vis.Network(container, data, options);
    }
};