using System;
using System.Collections.Generic;
using System.Diagnostics; // Necesario para Stopwatch
using System.Linq; // Necesario para usar LINQ
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
                    
                    switch (i) // Usar el índice 'i' para decidir el color
                    {
                        case 0:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case 1:
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                        case 2:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;
                        case 3:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                    }

                    Console.WriteLine($"{partitionMessages[i]} Proceso de tamaño {process.Size} asignado a la partición {i} (tamaño de partición: {partitionSizes[i]}) por {process.ExecutionTime} ms.");
                    Console.ResetColor(); // Restaurar el color después de la impresión
                    return i;
                }
            }
            return -1; // No se encontró una partición adecuada
        }
    }

    // Método para liberar la memoria ocupada por un proceso
    public void FreeMemory(int partitionIndex)
    {
        lock (lockObj) // Bloquear para evitar condiciones de carrera
        {
            if (partitionIndex < 0 || partitionIndex >= bitMap.Length)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Índice de partición no válido.");
                Console.ResetColor();
                return;
            }

            if (bitMap[partitionIndex] == 1) // Si el bit es 1, la partición está ocupada
            {
                bitMap[partitionIndex] = 0; // Marcar la partición como libre
                
                // Cambiar el color de la consola según la partición que se libera
                switch (partitionIndex)
                {
                    case 0:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case 1:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    case 2:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        break;
                    case 3:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    default:
                        Console.ResetColor(); // Resetear el color en caso de un índice inesperado
                        break;
                }
                
                Console.WriteLine($"Partición {partitionIndex} liberada.");
                Console.ResetColor(); // Restaurar el color después de la impresión
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Partición {partitionIndex} ya está libre.");
                Console.ResetColor();
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
        List<Process> processList = new List<Process>();
        Random random = new Random();

        // Generar 300 procesos con tamaños y tiempos de ejecución aleatorios y agregarlos a la lista
        for (int i = 0; i < numberOfProcesses; i++)
        {
            int processSize = random.Next(1, 401); // Tamaño aleatorio del proceso entre 1 y 400
            int executionTime = random.Next(1000, 5000); // Tiempo de ejecución aleatorio en milisegundos
            processList.Add(new Process(processSize, executionTime));
        }

        // Ordenar la lista de procesos por tiempo de ejecución de forma ascendente
        var orderedProcesses = processList.OrderBy(p => p.ExecutionTime);

        // Encolar los procesos ordenados
        Queue<Process> processQueue = new Queue<Process>(orderedProcesses);

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
