using System;
using System.Collections.Generic;
using System.Diagnostics; // Necesario para Stopwatch
using System.Threading;

class Process
{
    public int Size { get; set; }
    public int ExecutionTime { get; set; }
    public int PartitionIndex { get; set; }

    public Process(int size, int executionTime)
    {
        Size = size;
        ExecutionTime = executionTime;
        PartitionIndex = -1;
    }
}

class MemoryManagement
{
    private int[] memoryPartitions; // Arreglo que representa las particiones de memoria
    private int[] bitMap; // Mapa de bits para el seguimiento de las particiones ocupadas
    private int[] partitionSizes; // Tamaño de cada partición

    public MemoryManagement(int[] partitionSizes)
    {
        this.partitionSizes = partitionSizes;
        memoryPartitions = new int[partitionSizes.Length];
        bitMap = new int[partitionSizes.Length];
    }

    // Método para asignar memoria a un proceso
    public int AllocateMemory(Process process)
    {
        // Buscar una partición libre en el mapa de bits que pueda alojar el proceso
        for (int i = 0; i < bitMap.Length; i++)
        {
            if (bitMap[i] == 0 && process.Size <= partitionSizes[i]) // Si el bit es 0 y el tamaño del proceso cabe en la partición
            {
                bitMap[i] = 1; // Marcar la partición como ocupada
                memoryPartitions[i] = process.Size; // Asignar el proceso a la partición
                process.PartitionIndex = i;
                Console.WriteLine($"Proceso de tamaño {process.Size} asignado a la partición {i} (tamaño de partición: {partitionSizes[i]}) por {process.ExecutionTime} ms.");
                return i;
            }
        }

        Console.WriteLine($"Proceso de tamaño {process.Size} no puede ser asignado, no hay partición disponible o es demasiado grande.");
        return -1;
    }

    // Método para liberar la memoria ocupada por un proceso
    public void FreeMemory(int partitionIndex)
    {
        if (partitionIndex < 0 || partitionIndex >= bitMap.Length)
        {
            Console.WriteLine("Índice de partición no válido.");
            return;
        }

        if (bitMap[partitionIndex] == 1) // Si el bit es 1, la partición está ocupada
        {
            bitMap[partitionIndex] = 0; // Marcar la partición como libre
            Console.WriteLine($"Partición {partitionIndex} liberada.");
        }
        else
        {
            Console.WriteLine($"Partición {partitionIndex} ya está libre.");
        }
    }

    // Método para mostrar el estado actual del mapa de bits
    public void DisplayBitMap()
    {
        Console.WriteLine("Estado del mapa de bits:");
        for (int i = 0; i < bitMap.Length; i++)
        {
            Console.WriteLine($"Partición {i} (tamaño {partitionSizes[i+1]}): {(bitMap[i] == 1 ? "Ocupada" : "Libre")}");
        }
    }
}

class Program
{
    static void Main()
    {
        int[] partitionSizes = { 150, 200, 350, 400 }; // Tamaños de las particiones
        int numberOfProcesses = 300; // Número de procesos

        MemoryManagement memoryManager = new MemoryManagement(partitionSizes);
        Queue<Process> processQueue = new Queue<Process>();
        Random random = new Random();

        // Generar 300 procesos con tamaños y tiempos de ejecución aleatorios y agregarlos a la cola
        for (int i = 0; i < numberOfProcesses; i++)
        {
            int processSize = random.Next(1, 401); // Tamaño aleatorio del proceso entre 1 y 400
            int executionTime = random.Next(100, 1000); // Tiempo de ejecución aleatorio en milisegundos
            processQueue.Enqueue(new Process(processSize, executionTime));
        }

        // Iniciar el cronómetro
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Asignar memoria a cada proceso en la cola y simular su ejecución
        while (processQueue.Count > 0)
        {
            Process process = processQueue.Dequeue(); // Extraer el proceso de la cola
            int partitionIndex = memoryManager.AllocateMemory(process);
            if (partitionIndex != -1)
            {
                // Simular la ejecución del proceso
                Thread.Sleep(process.ExecutionTime);

                // Liberar la partición después de que el proceso haya terminado
                memoryManager.FreeMemory(partitionIndex);
            }
        }

        // Detener el cronómetro
        stopwatch.Stop();

        // Mostrar el estado final del mapa de bits
        memoryManager.DisplayBitMap();

        // Mostrar el tiempo total de ejecución
        Console.WriteLine($"Tiempo total de ejecución: {stopwatch.ElapsedMilliseconds} ms");
    }
}
