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
    private string[] partitionMessages; // Mensajes personalizados para cada partición
    private object lockObj = new object(); // Objeto para el bloqueo de secciones críticas

    public MemoryManagement(int[] partitionSizes, string[] partitionMessages)
    {
        Array.Sort(partitionSizes); // Ordenar las particiones de menor a mayor
        this.partitionSizes = partitionSizes;
        this.partitionMessages = partitionMessages;
        memoryPartitions = new int[partitionSizes.Length];
        bitMap = new int[partitionSizes.Length];
    }

    // Método para asignar memoria a un proceso
    public int AllocateMemory(Process process)
    {
        lock (lockObj) // Bloquear para evitar condiciones de carrera
        {
            // Buscar una partición libre en el mapa de bits que pueda alojar el proceso
            for (int i = 0; i < bitMap.Length; i++)
            {
                if (bitMap[i] == 0 && process.Size <= partitionSizes[i]) // Si el bit es 0 y el tamaño del proceso cabe en la partición
                {
                    bitMap[i] = 1; // Marcar la partición como ocupada
                    memoryPartitions[i] = process.Size; // Asignar el proceso a la partición
                    process.PartitionIndex = i;
                    Console.WriteLine($"{partitionMessages[i]} Proceso de tamaño {process.Size} asignado a la partición {i} (tamaño de partición: {partitionSizes[i]}) por {process.ExecutionTime} ms.");
                    return i;
                }
            }
            return -1;
        }
    }

    // Método para liberar la memoria ocupada por un proceso
    public void FreeMemory(int partitionIndex)
    {
        lock (lockObj) // Bloquear para evitar condiciones de carrera
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
    }

    // Método para mostrar el estado actual del mapa de bits
    public void DisplayBitMap()
    {
        lock (lockObj) // Bloquear para evitar condiciones de carrera
        {
            Console.WriteLine("Estado del mapa de bits:");
            for (int i = 0; i < bitMap.Length; i++)
            {
                Console.WriteLine($"Partición {i} (tamaño {partitionSizes[i]}): {(bitMap[i] == 1 ? "Ocupada" : "Libre")}");
            }
        }
    }

    // Método para ejecutar el proceso en un hilo separado
    public void RunProcess(Process process)
    {
        while (true)
        {
            int partitionIndex = AllocateMemory(process);
            if (partitionIndex != -1)
            {
                // Simular la ejecución del proceso
                Thread.Sleep(process.ExecutionTime);

                // Liberar la partición después de que el proceso haya terminado
                FreeMemory(partitionIndex);
                break;
            }
            else
            {
                // Esperar un corto período antes de intentar asignar memoria nuevamente
                Thread.Sleep(50);
            }
        }
    }
}

class Program
{
    static void Main()
    {
        int[] partitionSizes = { 150, 200, 350, 400 }; // Tamaños de las particiones
        string[] partitionMessages = 
        {
            "Partición pequeña:", 
            "Partición mediana:", 
            "Partición grande:", 
            "Partición extra grande:" 
        }; // Mensajes personalizados para cada partición
        int numberOfProcesses = 300; // Número de procesos

        MemoryManagement memoryManager = new MemoryManagement(partitionSizes, partitionMessages);
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

        // Ejecutar cada proceso en un hilo separado
        List<Thread> threads = new List<Thread>();

        while (processQueue.Count > 0)
        {
            Process process = processQueue.Dequeue(); // Extraer el proceso de la cola
            Thread thread = new Thread(() => memoryManager.RunProcess(process));
            threads.Add(thread);
            thread.Start();
        }

        // Esperar a que todos los hilos terminen
        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Detener el cronómetro
        stopwatch.Stop();

        // Mostrar el estado final del mapa de bits
        memoryManager.DisplayBitMap();

        // Mostrar el tiempo total de ejecución
        Console.WriteLine($"Tiempo total de ejecución: {stopwatch.ElapsedMilliseconds} ms");
    }
}
