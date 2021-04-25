#
#   Hello World server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#

import time
import zmq
import networkx as nx
from networkx.algorithms import community
import itertools
import hypernetx as hnx
from hypernetx.classes.entity import Entity, EntitySet
import argparse, json, os
import subprocess   

from itertools import combinations


context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:8080")

print("Server_loaded")

def packJson(G):
    edges = []
    for cur_edge in G.edges():
        cur_edge = list(cur_edge)

        edges.append({"edge_start":cur_edge[0], "edge_end":cur_edge[-1], "weight":1})


    graph = {"edges":edges,"simplicials":[],"hyperedges":[],"nodes":list(G.nodes())}
    print(graph)

    with open("../../Resources/output.json", 'w') as json_file:
        json.dump(graph, json_file)

def unpackJson():
    
    f = open("../../Resources/data.json",)
    graph_dict = json.load(f) 
    #print(graph_dict) 
    
    all_graphs = []
        
    for cur_graph in graph_dict['graphs']:

        each_graph = {"node": None, "edges": None, "simplicials": None, "hyperedges": None}

        graph_edge_list = []    
        for cur_edge in cur_graph['edges']:
            temp_edge = []
            temp_edge.append(cur_edge['edge_start'])
            temp_edge.append(cur_edge['edge_end'])
            graph_edge_list.append((temp_edge))

        simplicial_edge_list = []    
        for cur_edge in cur_graph['simplicials']:
            temp_edge = cur_edge['nodes']        
            simplicial_edge_list.append((temp_edge))    

        hypergraph_edge_list = []    
        for cur_edge in cur_graph['hyperedges']:
            temp_edge = cur_edge['nodes']        
            hypergraph_edge_list.append((temp_edge))

        each_graph["node"] = cur_graph['nodes']
        each_graph["edges"] = graph_edge_list
        each_graph["simplicials"] = simplicial_edge_list
        each_graph["hyperedges"] = hypergraph_edge_list

        all_graphs.append(each_graph)
        #print(each_graph)   
        
    return all_graphs

def unpackWeightedJson():
    
    f = open("../../Resources/data.json",)
    graph_dict = json.load(f) 
    #print(graph_dict) 
    
     
    for cur_graph in graph_dict['graphs']:
        
        G = nx.Graph()
        G.add_nodes_from(cur_graph['nodes'])
        
        for cur_edge in cur_graph['edges']:
            G.add_edge(cur_edge['edge_start'], cur_edge['edge_end'], weight=cur_edge['weight'])
                  
        return G

def packGraphConversionJson(nodes, frozenedges, simplices, hyper):
    
    edges = []
    frozenedges = list(frozenedges)
    for cur_edge in frozenedges:
        cur_edge = list(cur_edge)
        edges.append({"edge_start":cur_edge[0], "edge_end":cur_edge[-1], "weight":1})
        
    simplicials = []
    simplices = list(simplices)
    for cur_edge in simplices:
        cur_edge = list(cur_edge)

        simplicials.append({"nodes":cur_edge, "weight":1})        
    
    hyperedges = []
    hyper = list(hyper)
    for cur_edge in hyper:
        cur_edge = list(cur_edge)
        hyperedges.append({"nodes":cur_edge, "weight":1})   


    graph = {"edges": edges, "simplicials": simplicials, "hyperedges": hyperedges, "nodes": list(nodes)}
    print(graph)

    with open("../../Resources/output.json", 'w') as json_file:
        json.dump(graph, json_file)

# format: {<nodes seperated by comma>-{edge_1}{edge_2}....}
def parse_message(sent_Str):
    sent_Str=sent_Str.split("-")
    print(sent_Str)

    node_Str=sent_Str[0][1:-1]
    print(node_Str)

    edge_Str=sent_Str[-1].split("}")
    print(edge_Str)

    nodes_list = []
    for each_node in node_Str.split(','):
        nodes_list.append(int(each_node))
    print(nodes_list)    

    edges_list = []
    for each_edge_Str in edge_Str:
        if each_edge_Str == '':
            continue
        each_edge_Str = each_edge_Str[1:]
        cur_edge = []
        for edge_node in each_edge_Str.split(','):
            cur_edge.append(int(edge_node))
        print(cur_edge)    
        edges_list.append(frozenset(cur_edge))

    print(edges_list)    
    return nodes_list, edges_list

def degree_centrality(all_graphs):
    G = nx.Graph()
    G.add_nodes_from(all_graphs[0]["node"])
    G.add_edges_from(all_graphs[0]["edges"])
    
    #try:
    nodes = nx.degree_centrality(G)
    nodes_pair = sorted(nodes.items(), key=lambda x: x[1], reverse=True)

    nodes = []
    for item in nodes_pair:
        nodes.append(item[0])

    graph = {"edges":[],"simplicials":[],"hyperedges":[],"nodes":nodes}
    print(graph)
        
#     except:
#         graph = {"edges":[],"simplicials":[],"hyperedges":[],"nodes":[]}
#         print(graph)

    with open("../../Resources/output.json", 'w') as json_file:
        json.dump(graph, json_file)
        
        
def community_detection(all_graphs):
    
    G = nx.Graph()
    G.add_nodes_from(all_graphs[0]["node"])
    G.add_edges_from(all_graphs[0]["edges"])
    
    try:
        communities_generator = community.girvan_newman(G)
        top_level_communities = next(communities_generator)
        next_level_communities = next(communities_generator)
        all_communities = sorted(map(sorted, next_level_communities))

        graphs = []
        for each_com in all_communities:
            graphs.append({"edges":[],"simplicials":[],"hyperedges":[],"nodes":each_com})
            
    except:
        graphs = []
        graphs.append({"edges":[],"simplicials":[],"hyperedges":[],"nodes":all_graphs[0]["node"]})
    
    final_json = {"graphs":graphs}
    print(final_json)
    with open("../../Resources/output.json", 'w') as json_file:
        json.dump(final_json, json_file)

def find_shortest_path(G, source, target):    
            
    try:
        nodes = nx.shortest_path(G, source = source, target = target, weight='weight')

        graph = {"edges":[],"simplicials":[],"hyperedges":[],"nodes":nodes}
        print(graph)
        
    except:
        graph = {"edges":[],"simplicials":[],"hyperedges":[],"nodes":[]}
        print(graph)

    with open("../../Resources/output.json", 'w') as json_file:
        json.dump(graph, json_file)
        
        
def find_shortest_path_length(G, source, target):    
            
    try:
        length = nx.shortest_path_length(G, source = source, target = target, weight='weight')
        return length
        
    except:
        return -1

def topological_sort(all_graphs):
        
    DG = nx.DiGraph(list(all_graphs[0]['edges']))
    nodes = list(nx.topological_sort(DG))
    print(nodes)
    
    graph = {"edges":[],"simplicials":[],"hyperedges":[],"nodes":nodes}
    print(graph)

    with open("../../Resources/output.json", 'w') as json_file:
        json.dump(graph, json_file)
    
    
def graph_addition(all_graphs):    
        
    G = nx.Graph()
    G.add_nodes_from(all_graphs[0]["node"])
    G.add_edges_from(all_graphs[0]["edges"])
    
    if (len(all_graphs) < 2):
        return G
    
    for iter in range(1, len(all_graphs)):
    
        G_2 = nx.Graph()
        G_2.add_nodes_from(all_graphs[iter]["node"])
        G_2.add_edges_from(all_graphs[iter]["edges"])

        G_new = nx.compose(G,G_2)
        G = G_new
    
    #plt.subplot(121)
    #nx.draw(G_new, with_labels=True, font_weight='bold')
    return G

def ego_graph(all_graphs, node, rad):    
        
    G = nx.Graph()
    G.add_nodes_from(all_graphs[0]["node"])
    G.add_edges_from(all_graphs[0]["edges"])
    
    try:
        H = nx.ego_graph(G, n=node, radius=rad)
        return H
        
    except:
        return G    
    
    
def Graph_getLayout(all_graphs, layout_type):
    
    G = nx.Graph()
    G.add_nodes_from(all_graphs[0]["node"])
    G.add_edges_from(all_graphs[0]["edges"])
    
    pos = []
    node_cord = []       
    
    try:
        if (layout_type == "circular"):
            pos=nx.circular_layout(G)
        
        elif (layout_type == "random"):
            pos=nx.random_layout(G)
        
        elif (layout_type == "spring"):
            pos=nx.spring_layout(G)
        
        elif (layout_type == "spectral"):
            pos=nx.spectral_layout(G)
        
        else:
            pos=nx.fruchterman_reingold_layout(G)            
            
        for each_pos in pos.keys():
            pixel_cord = pos[each_pos]
            #node_cord.append({"node_id": each_pos, "x": str(round(pixel_cord[0],3)), "y": str(round(pixel_cord[1],3))})
            node_cord.append({"node_id": each_pos, "x": str(pixel_cord[0]), "y": str(pixel_cord[1])})
        
    except:         
        node_cord.append({"node_id": -1, "x": str(-1), "y": str(-1)})
        
    final_json = {"node_cord": node_cord}
    print(final_json)
    with open("../../Resources/output.json", 'w') as json_file:
        json.dump(final_json, json_file)
    
    return G, pos

## So we can have an empty edge/face/simplex set everywhere, even the
## node set can be empty
## TODO: 
## Understand whether simplicial complex constrction from graph has
## some complex procudeure by looking at neighborhood and cliques
## Then try to understand if we really need point simplices in the
## simplicial complex. If the is really required, do we need to put
## edges less than 2 nodes in hypergraph?
## Another question, do we create a point simplex if it just a dangling node
## in simplicial complex?

## Node and edge subtraction
## So if we remove triangle we just keep the traingle but keeps the edges

class Graph:
    """
    A Graph is a tuple (nodes,edges) where the element of nodes are called nodes
    or vertices and each element e of edges are called edge where |e| =2
    for all e.
    """
    def __init__(self, nodes=None, edges=None):
        self.nodes = set()
        self.edges = set()
        self.G = nx.Graph()
        if nodes:
            for node in nodes:
                self.add_node(node)
                self.G.add_node(node)
        if edges:
            for edge in edges:
                self.add_edge(edge)
                self.G.add_edge(*edge)
    
    def get_cliques(self, k = 3):
        
#         temp_edges = set()
#         for each_edge in set(self.G.edges):
#             temp_edges.add()
        temp_edges = self.edges.copy()        
        cliques = set()
        
        for clique in nx.find_cliques(self.G):
            if len(clique) == k:
                cliques.add(frozenset(clique))
                
                edges = list(itertools.combinations(clique, 2))
                for possible_edge in edges:
                    possible_edge = frozenset(possible_edge)
#                     print("possible_edge",possible_edge)
                    if possible_edge in temp_edges:
                        temp_edges.remove(possible_edge)
                
            elif len(clique) > k:
                subset_cliques = list(itertools.combinations(clique, k))
                for subset_clique in subset_cliques:
                    cliques.add(frozenset(subset_clique))
                    
                edges = list(itertools.combinations(clique, 2))
                for possible_edge in edges:
                    if possible_edge in temp_edges:
                        temp_edges.remove(possible_edge)
                        
        #print(temp_edges)
                
        return list(cliques.union(temp_edges))
            
    def add_node(self,node):
        self.nodes.add(node)

    def add_edge(self,edge):
        """
        Add an edge to the set of edges
        """
        self.edges.add(frozenset(edge))
        for node in edge:
            self.add_node(node)

    def to_simplicial_complex(self):
        pass
        nodes = self.nodes
        simplices = self.get_cliques() #self.edges
        return(SimplicialComplex(nodes=nodes, simplices=simplices))

    def to_hypergraph(self):
        pass
        nodes = self.nodes
        hyperedges = self.edges
        return(HyperGraph(nodes=nodes, hyperedges=hyperedges))

def get_subsets(s, n): 
    return [frozenset(i) for i in combinations(s, n)] 

def frozen_set_to_list(frozenlist):
    sets=list(frozenlist)
    #edges = [list(x) for x in sets]
    
    final_str = ""
    for iter_1, each_edge in enumerate(sets):
        #print(list(each_edge))

        edges_as_str = ""
        for iter, each_node in enumerate(list(each_edge)):
            if iter != 0:
                edges_as_str += "," + str(each_node)
            else:
                edges_as_str += str(each_node)

        if iter_1 != 0:
            final_str += "-" + edges_as_str
        else:
            final_str += edges_as_str

    print(final_str)
    return final_str

class SimplicialComplex:
    """
    A simplicial complex is a tuple (V, S) where the elements of V are
    called nodes or vertices, and each element s of S is called a simplex.
    Each s is a subset of V. Downward inclusion property of simplicial
    complex dictates that if s is a subset of S and r is a subset of s
    then r must be inside S

    """
    def __init__(self, nodes=None, simplices=None):
        self.nodes = set()
        self.simplices = set()
        if nodes:
            for node in nodes:
                self.add_node(node)
        if simplices:
            ## TODO add combinations
            for simplex in simplices:
                self.add_simplex(simplex)

    def add_node(self,node):
        self.nodes.add(node)

    def add_simplex(self,simplex):
        """
        Add a simplex to the simplicial complex
        """
        self.simplices.add(frozenset(simplex))
        ## To make sure the downward inclusion
        ## we are adding all the subsets of the simplex
        ## in the simplces set
        ## for i in range(1,len(simplex)):
        ## We do not want any simplex that is empty and
        ## we have already put the whole simplex itself in
        ## the line outside the loop
            ## self.simplices.update(get_subsets(simplex,i))
        for node in simplex:
            self.add_node(node)

    def to_hypergraph(self):
        ## TODO need to make sure whether 1 simplices
        ## or the point simplices should be in hyper graph or not
        pass
        nodes = self.nodes
        hyperedges = [e for e in self.simplices if len(e)>1]
        return HyperGraph(nodes=nodes,hyperedges=hyperedges)

    def to_graph(self):
        pass
        nodes = self.nodes
        edges = []
        for simplex in self.simplices:
            if len(simplex) < 2:
                pass
                ## We are not keeping the point simplices
            elif len(simplex) == 2:
                edges.append(simplex)
            else:
                ## We are creating 2 length subsets to 
                ## the edge set
                edges.extend(get_subsets(simplex,2))
        return Graph(nodes=nodes,edges=edges)

class HyperGraph:
    def __init__(self, nodes=None, hyperedges=None):
        self.hyperedges = set()
        self.H = hnx.Hypergraph()
        self.unique_edge_number = 0
        self.edge_to_unique_edge_number = dict()
        if hyperedges:
            for hyperedge in hyperedges:
                #print(hyperedge)
                self.add_hyperedge(hyperedge)
        if nodes:
            for node in nodes:
                self.add_node(node)
                
        self.get_hyperedges()
        self.nodes = self.get_nodes()

    def get_nodes(self):
        return set(self.H.nodes)

    def get_hyperedges(self):
        self.hyperedges = list(map(frozenset,self.H.incidence_dict.values()))
        return self.hyperedges #list(map(frozenset,self.H.incidence_dict.values()))

    def add_node(self,node):
        self.H._nodes.add(Entity(node))

    def add_hyperedge(self,hyperedge):
        """
        Add a hyperedge.
        Params
        ------
        hyperedge (set): a set of nodes that have interaction
        with each other
        """
        self.H.add_edge(Entity("_%d" %self.unique_edge_number,hyperedge))
        ## Each of the edge has a unique edge id, also the same 
        ## edge id can not be a node itself, so we will put an
        ## underscore before the unique integer and make it a string
        self.edge_to_unique_edge_number[frozenset(hyperedge)] = "_%d" %self.unique_edge_number
        self.unique_edge_number += 1

    def to_simplicial_complex(self):
        pass
        nodes = self.get_nodes()
        simplices = self.get_hyperedges()
        return SimplicialComplex(nodes=nodes,simplices=simplices)

    def to_graph(self):
        pass
        nodes = self.get_nodes()
        edges = []
        for hyperedge in self.get_hyperedges():
            ## The following is redundant, we should not 
            ## have a 1 length hyperedge, but keeping
            ## it if it changes in the future
            if len(hyperedge) < 2:
                pass
                ## We are not keeping the point simplices
            elif len(hyperedge) == 2:
                edges.append(hyperedge)
            else:
                ## We are creating 2 length subsets to 
                ## the edge set
                edges.extend(get_subsets(hyperedge,2))
        return Graph(nodes=nodes,edges=edges)


if __name__ == '__main__':
    #args = parser.parse_args()    
    
    while True:
        #  Wait for next request from client
        message = socket.recv()
        
        print("Received request from unity: %s" % message)
        #  Do some 'work'.
        #  Try reducing sleep time to 0.01 to see how blazingly fast it communicates
        #  In the real world usage, you just need to replace time.sleep() with
        #  whatever work you want python to do, maybe a machine learning task?
        #  time.sleep(1)  
    
        
        if message.decode('utf8') == "addition":
            all_graphs = unpackJson() #parse_list_message(message.decode('utf8'))
            G = graph_addition(all_graphs)
            if G is not None:
                packJson(G)
                socket.send(("addition").encode('ascii'))  
            else:            
                socket.send(("impossible").encode('ascii'))
                
        elif message.decode('utf8') == "topologicalsort":
            all_graphs = unpackJson()
            topological_sort(all_graphs)
            socket.send(("topologicalsort").encode('ascii'))
            
        elif message.decode('utf8') == "degreesort":
            all_graphs = unpackJson()
            degree_centrality(all_graphs)
            socket.send(("degreesort").encode('ascii')) 
            
        elif message.decode('utf8') == "community":
            all_graphs = unpackJson()
            community_detection(all_graphs)
            socket.send(("community").encode('ascii'))  
            
        elif ("egograph" in message.decode('utf8') or "neighborgraph" in message.decode('utf8')):
            args = message.decode('utf8').split("_")
            node, rad =  args[1], args[-1]
            all_graphs = unpackJson() 
            G = ego_graph(all_graphs, int(node), int(rad))
            #print("ego_evaluation_done")
            packJson(G)
            socket.send(("egograph").encode('ascii'))  
            
        elif "shortestpathlength" in message.decode('utf8'):
            all_graphs = unpackWeightedJson()
            args = message.decode('utf8').split("_")
            source, target =  args[1], args[-1]
            path_length = find_shortest_path_length(all_graphs, int(source), int(target))
            socket.send(str(path_length).encode('ascii'))
            
        elif "shortestpath" in message.decode('utf8'):
            all_graphs = unpackWeightedJson()
            args = message.decode('utf8').split("_")
            source, target =  args[1], args[-1]
            find_shortest_path(all_graphs, int(source), int(target))
            socket.send(("shortestpath").encode('ascii'))
            
        elif "layout" in message.decode('utf8'):
            all_graphs = unpackJson()
            args = message.decode('utf8').split("_")
            Graph_getLayout(all_graphs, args[-1])
            socket.send(("layout").encode('ascii')) 
            
        elif message.decode('utf8') == "graph_to_simplicial":
            #TypeError: a bytes-like object is required, not 'str'
            #nodes_list, edges_list = parse_message(message.decode('utf8'))
            all_graphs = unpackJson()
            nodes_list, edges_list = all_graphs[0]["node"], all_graphs[0]["edges"]
            graph = Graph(nodes_list, edges_list)
            graph_to_simplicial_complex = graph.to_simplicial_complex()
            print("graph_to_simplicial_complex",graph_to_simplicial_complex.simplices)
            packGraphConversionJson(graph_to_simplicial_complex.nodes, [], graph_to_simplicial_complex.simplices, [])
            socket.send((frozen_set_to_list(graph_to_simplicial_complex.simplices)).encode('ascii'))  

        elif message.decode('utf8') == "graph_to_hypergraph":
            #TypeError: a bytes-like object is required, not 'str'
            #nodes_list, edges_list = parse_message(message.decode('utf8'))
            all_graphs = unpackJson()
            nodes_list, edges_list = all_graphs[0]["node"], all_graphs[0]["edges"]
            graph = Graph(nodes_list, edges_list)
            graph_to_hypergraph = graph.to_hypergraph()
            print("graph_to_hypergraph",graph_to_hypergraph.hyperedges)
            packGraphConversionJson(graph_to_hypergraph.nodes, [], [], graph_to_hypergraph.hyperedges)
            socket.send((frozen_set_to_list(graph_to_hypergraph.hyperedges)).encode('ascii'))    

        elif message.decode('utf8') == "hypergraph_to_graph":
            #TypeError: a bytes-like object is required, not 'str'
            #nodes_list, edges_list = parse_message(message.decode('utf8'))
            all_graphs = unpackJson()
            nodes_list, edges_list = all_graphs[0]["node"], all_graphs[0]["hyperedges"]
            hypergraph = HyperGraph(nodes_list, edges_list)
            hypergraph_to_graph = hypergraph.to_graph()
            print("hypergraph_to_graph",hypergraph_to_graph.edges)
            packGraphConversionJson(hypergraph_to_graph.nodes, hypergraph_to_graph.edges, [], [])
            socket.send((frozen_set_to_list(hypergraph_to_graph.edges)).encode('ascii'))  

        elif message.decode('utf8') == "hypergraph_to_simplicial":
            #TypeError: a bytes-like object is required, not 'str'
            #nodes_list, edges_list = parse_message(message.decode('utf8'))
            all_graphs = unpackJson()
            nodes_list, edges_list = all_graphs[0]["node"], all_graphs[0]["hyperedges"]
            hypergraph = HyperGraph(nodes_list, edges_list)
            hypergraph_to_simplicial = hypergraph.to_simplicial_complex()
            print("hypergraph_to_simplicial",hypergraph_to_simplicial.simplices)
            packGraphConversionJson(hypergraph_to_simplicial.nodes, [], hypergraph_to_simplicial.simplices, [])
            socket.send((frozen_set_to_list(hypergraph_to_simplicial.simplices)).encode('ascii'))  

        elif message.decode('utf8') == "simplicial_to_graph":
            #TypeError: a bytes-like object is required, not 'str'
            #nodes_list, edges_list = parse_message(message.decode('utf8'))
            all_graphs = unpackJson()
            nodes_list, edges_list = all_graphs[0]["node"], all_graphs[0]["simplicials"]
            simplicial = SimplicialComplex(nodes_list, edges_list)
            simplicial_to_graph = simplicial.to_graph()
            print("simplicial_to_graph",simplicial_to_graph.edges)
            packGraphConversionJson(simplicial_to_graph.nodes, simplicial_to_graph.edges, [], [])
            socket.send((frozen_set_to_list(simplicial_to_graph.edges)).encode('ascii'))  

        elif message.decode('utf8') == "simplicial_to_hypergraph":
            #TypeError: a bytes-like object is required, not 'str'
            #nodes_list, edges_list = parse_message(message.decode('utf8'))
            all_graphs = unpackJson()
            nodes_list, edges_list = all_graphs[0]["node"], all_graphs[0]["simplicials"]
            simplicial = SimplicialComplex(nodes_list, edges_list)
            simplicial_to_hypergraph = simplicial.to_hypergraph()
            print("simplicial_to_hypergraph",simplicial_to_hypergraph.hyperedges)
            packGraphConversionJson(simplicial_to_hypergraph.nodes, [], [], simplicial_to_hypergraph.hyperedges)
            socket.send((frozen_set_to_list(simplicial_to_hypergraph.hyperedges)).encode('ascii'))  