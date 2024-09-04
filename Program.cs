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
        int numberOfProcesses = 40; // Número de procesos

        MemoryManagement memoryManager = new MemoryManagement(partitionSizes, partitionMessages);
        Queue<Process> processQueue = new Queue<Process>();

        // Procesos predefinidos (tamaños y tiempos de ejecución)
        int[,] predefinedProcesses = new int[,]
        {
            { 50, 2000 }, { 120, 1500 }, { 90, 3000 }, { 200, 2500 },
            { 150, 1000 }, { 300, 3500 }, { 170, 1200 }, { 110, 1800 },
            { 80, 2200 }, { 60, 1700 }, { 200, 4000 }, { 130, 2300 },
            { 100, 1400 }, { 180, 2100 }, { 50, 3000 }, { 90, 2600 },
            { 160, 1900 }, { 140, 2400 }, { 120, 1300 }, { 210, 2800 },
            { 180, 1500 }, { 170, 3100 }, { 90, 1800 }, { 200, 3500 },
            { 100, 1300 }, { 300, 3400 }, { 150, 1700 }, { 160, 2500 },
            { 70, 2100 }, { 140, 2700 }, { 110, 1500 }, { 50, 1200 },
            { 200, 2300 }, { 90, 3000 }, { 150, 2500 }, { 130, 2800 },
            { 120, 3300 }, { 300, 3500 }, { 140, 2900 }, { 180, 3700 }
        };

        // Agregar los procesos predefinidos a la cola
        for (int i = 0; i < predefinedProcesses.GetLength(0); i++)
        {
            int processSize = predefinedProcesses[i, 0];
            int executionTime = predefinedProcesses[i, 1];
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
