using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;

class Program
{
    static Dictionary<(int, int), List<(int, int)>> CreateGraph(int size)
    {
        var graph = new Dictionary<(int, int), List<(int, int)>>();

        // Definisci i movimenti consentiti
        var moves = new (int, int)[]
        {
            (3, 0), (-3, 0),  // Movimenti orizzontali (saltando 2 caselle)
            (0, 3), (0, -3),  // Movimenti verticali (saltando 2 caselle)
            (2, 2), (2, -2), (-2, 2), (-2, -2)  // Movimenti diagonali (saltando 1 casella)
        };

        // Crea i nodi e gli archi del grafo
        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                var node = (r, c);
                graph[node] = new List<(int, int)>();

                foreach (var move in moves)
                {
                    var neighbor = (r + move.Item1, c + move.Item2);
                    if (neighbor.Item1 >= 0 && neighbor.Item1 < size && neighbor.Item2 >= 0 && neighbor.Item2 < size)
                    {
                        graph[node].Add(neighbor);
                    }
                }
            }
        }

        return graph;
    }

    static List<(int, int)> FindSolution(Dictionary<(int, int), List<(int, int)>> graph, (int, int) startNode, int gridSize)
    {
        var path = new List<(int, int)>();
        var visited = new HashSet<(int, int)>();

        bool Dfs((int, int) node)
        {
            visited.Add(node);
            path.Add(node);

            if (visited.Count == gridSize * gridSize)
            {
                return true;
            }

            var neighbors = graph[node]
                .OrderBy(n => graph[n].Count(nbr => !visited.Contains(nbr)))
                .ToList();

            foreach (var neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    if (Dfs(neighbor))
                    {
                        return true;
                    }
                }
            }

            // Backtrack
            visited.Remove(node);
            path.RemoveAt(path.Count - 1);
            return false;
        }

        Dfs(startNode);
        return path;
    }

    static void PrintPath(List<(int, int)> path, int size)
    {
        var grid = new int[size, size];
        for (int i = 0; i < path.Count; i++)
        {
            var pos = path[i];
            grid[pos.Item1, pos.Item2] = i + 1;
        }

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                Console.Write($"{grid[r, c],3} ");
            }
            Console.WriteLine();
        }
    }

    static void AnimatePath(List<(int, int)> path, int size)
    {
        // Inizializza la griglia vuota
        var grid = new int[size, size];

        // Posiziona il cursore sotto l'output precedente
        Console.WriteLine("\nAnimazione del percorso:");

        // Stampa la griglia vuota
        int startRow = Console.CursorTop; // Salva la riga corrente per l'animazione
        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                Console.Write("... ");
            }
            Console.WriteLine();
        }

        // Riempie gradualmente la griglia con il percorso
        for (int i = 0; i < path.Count; i++)
        {
            var pos = path[i];
            grid[pos.Item1, pos.Item2] = i + 1;

            // Calcola le prossime mosse possibili
            var possibleMoves = new List<(int, int)>();
            foreach (var move in new (int, int)[] { (3, 0), (-3, 0), (0, 3), (0, -3), (2, 2), (2, -2), (-2, 2), (-2, -2) })
            {
                var nextPos = (pos.Item1 + move.Item1, pos.Item2 + move.Item2);
                if (nextPos.Item1 >= 0 && nextPos.Item1 < size && nextPos.Item2 >= 0 && nextPos.Item2 < size && grid[nextPos.Item1, nextPos.Item2] == 0)
                {
                    possibleMoves.Add(nextPos);
                }
            }

            // Evidenzia le prossime mosse possibili in verde
            foreach (var move in possibleMoves)
            {
                Console.SetCursorPosition(move.Item2 * 4, startRow + move.Item1);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" *  ");
            }

            // Posiziona il cursore e aggiorna il valore nella griglia
            Console.SetCursorPosition(pos.Item2 * 4, startRow + pos.Item1);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{grid[pos.Item1, pos.Item2],3} ");

            // Evidenzia i punti non raggiungibili in rosso
            foreach (var r in Enumerable.Range(0, size))
            {
                foreach (var c in Enumerable.Range(0, size))
                {
                    if (grid[r, c] == 0 && !possibleMoves.Contains((r, c)))
                    {
                        Console.SetCursorPosition(c * 4, startRow + r);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("... ");
                    }
                }
            }

            // Introduce un ritardo per simulare l'animazione
            Thread.Sleep(500); // 500 ms di ritardo
            while (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true).Key;
                if (key == ConsoleKey.Spacebar)
                {
                    Console.SetCursorPosition(0, startRow + size + 2);
                    Console.WriteLine("Animazione in pausa. Premi la barra spaziatrice per continuare...");
                    while (Console.ReadKey(intercept: true).Key != ConsoleKey.Spacebar)
                    {
                        // Aspetta che l'utente prema nuovamente la barra spaziatrice
                    }
                    Console.SetCursorPosition(0, startRow + size + 2);
                    Console.WriteLine(new string(' ', Console.WindowWidth));
                }
            } 
        }

        // Ripristina il colore originale
        Console.ForegroundColor = ConsoleColor.White;

        // Posiziona il cursore alla fine per evitare sovrapposizioni
        Console.SetCursorPosition(0, startRow + size + 1);
    }

    static void Main(string[] args)
    {
        // Definisci la dimensione della griglia
        int gridSize = 10;

        // start stopwatch
        var stopwatch = Stopwatch.StartNew();

        // Creazione del campo di gioco
        var graph = CreateGraph(gridSize);

        // Sceglie una posizione iniziale a caso
        var random = new Random();
        var startNode = (random.Next(0, gridSize), random.Next(0, gridSize));
        Console.Clear();
        Console.WriteLine($"Posizione di partenza: {startNode}");

        // chiama la funzione di ricerca della soluzione
        // questa soluzione non usa la ricorsione, ma è un algoritmo DFS iterativo Depth-First Search
        // in matematica è detto Cammino hamiltoniano?
        var path = FindSolution(graph, startNode, gridSize);

        // Stampa il percorso trovato
        PrintPath(path, gridSize);

        // stop stopwatch e stampa il tempo impiegato
        stopwatch.Stop();
        Console.WriteLine($"Soluzione trovata in : {stopwatch.ElapsedMilliseconds} ms\n");

        // Anima il percorso già elaborato per visualizzarlo graficamente a velocità umana 
        Console.WriteLine("Ora ti faccio vedere l'elaborazione che ho calcolato:\n");
        AnimatePath(path, gridSize);
    }
}
