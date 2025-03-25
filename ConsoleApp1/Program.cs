using System;
using System.Collections.Generic;
using System.Linq;

namespace ShippingManagement
{
    interface IHazardNotifier
    {
        void NotifyHazard(string message);
    }

    abstract class Container : IHazardNotifier
    {
        public string SerialNumber { get; protected set; }
        public string Type { get; protected set; }
        public double MaxCapacity { get; protected set; }
        public double TareWeight { get; protected set; }
        public double CurrentLoad { get; protected set; }
        public double LoadWeight { get; set; }
        public bool IsHazardous { get; protected set; }

        private static int serialCounter = 1;

        public Container(string type, double maxCapacity, double tareWeight, bool isHazardous)
        {
            Type = type;
            MaxCapacity = maxCapacity;
            TareWeight = tareWeight;
            IsHazardous = isHazardous;
            SerialNumber = $"KON-{type}-{serialCounter++}";
            CurrentLoad = 0;
            LoadWeight = 0; 
        }

        public void Load(double weight)
        {
            if (weight <= 0) throw new ArgumentException("Waga musi być większa niż zero.");
            double maxAllowedLoad = IsHazardous ? MaxCapacity * 0.5 : MaxCapacity * 0.9;

            if (CurrentLoad + weight > maxAllowedLoad)
            {
                NotifyHazard($"Próba przepełnienia kontenera {SerialNumber}!");
                throw new InvalidOperationException("Próba przepełnienia kontenera.");
            }

            CurrentLoad += weight;
        }

        public void SetLoadWeight(double weight)
        {
            if (weight <= 0)
            {
                throw new ArgumentException("Załadunek musi mieć wagę większą niż zero.");
            }
            LoadWeight = weight;
        }

        public virtual void Unload()
        {
            CurrentLoad = 0;
            LoadWeight = 0;
        }

        public void NotifyHazard(string message)
        {
            Console.WriteLine($"ALERT: {message}");
        }

        public double TotalWeight()
        {
            return TareWeight + LoadWeight + CurrentLoad;
        }
    }

    class LiquidContainer : Container
    {
        public LiquidContainer(double maxCapacity, double tareWeight, bool isHazardous)
            : base("L", maxCapacity, tareWeight, isHazardous) { }

        public override void Unload()
        {
            base.Unload();
            Console.WriteLine($"Kontener {SerialNumber} rozładowany.");
        }
    }

    class GasContainer : Container
    {
        public double Pressure { get; private set; }

        public GasContainer(double maxCapacity, double tareWeight, double pressure)
            : base("G", maxCapacity, tareWeight, true)
        {
            Pressure = pressure;
        }

        public override void Unload()
        {
            CurrentLoad *= 0.05; 
            NotifyHazard($"Po rozładunku w kontenerze {SerialNumber} pozostało 5% ładunku.");
            Console.WriteLine($"Kontener {SerialNumber} rozładowany.");
        }
    }

    class RefrigeratedContainer : Container
    {
        public double Temperature { get; private set; }

        public RefrigeratedContainer(double maxCapacity, double tareWeight, double temperature)
            : base("C", maxCapacity, tareWeight, false)
        {
            Temperature = temperature;
        }

        public override void Unload()
        {
            base.Unload();
            Console.WriteLine($"Kontener {SerialNumber} rozładowany.");
        }
    }

    class Ship
    {
        public string Name { get; set; }
        public double MaxSpeed { get; set; }
        public int MaxContainers { get; set; }
        public double MaxWeight { get; set; }
        public List<Container> Containers { get; set; } = new List<Container>();

        public Ship(string name, double maxSpeed, int maxContainers, double maxWeight)
        {
            Name = name;
            MaxSpeed = maxSpeed;
            MaxContainers = maxContainers;
            MaxWeight = maxWeight;
        }

        public double CurrentWeight()
        {
            return Containers.Sum(c => c.TotalWeight()) / 1000.0; 
        }

        public double CapacityUsage() => (CurrentWeight() / MaxWeight) * 100;

        public void AddContainer(Container container)
        {
            if (Containers.Count >= MaxContainers || CurrentWeight() + container.TotalWeight() / 1000.0 > MaxWeight)
            {
                Console.WriteLine("Nie można dodać kontenera – statek przekroczył limit.");
                return;
            }

            Containers.Add(container);
            Console.WriteLine($"Dodano kontener {container.SerialNumber} na statek {Name}.");
        }

        public void RemoveContainer(string serialNumber)
        {
            var container = Containers.FirstOrDefault(c => c.SerialNumber == serialNumber);
            if (container != null)
            {
                Containers.Remove(container);
                Console.WriteLine($"Usunięto kontener {serialNumber} ze statku {Name}.");
            }
            else
            {
                Console.WriteLine("Nie znaleziono kontenera o podanym numerze seryjnym.");
            }
        }
    }

    class Program
    {
        static List<Ship> ships = new List<Ship>();
        static List<Container> containers = new List<Container>();

        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Dodaj statek");
                Console.WriteLine("2. Dodaj kontener");
                Console.WriteLine("3. Załaduj kontener na statek");
                Console.WriteLine("4. Rozładuj kontener");
                Console.WriteLine("5. Wyświetl statki");
                Console.WriteLine("6. Wyświetl kontenery");
                Console.WriteLine("7. Zastąp kontener na statku");
                Console.WriteLine("8. Przenieś kontener między statkami");
                Console.WriteLine("9. Wyjście");
                Console.Write("Wybierz opcję: ");

                switch (Console.ReadLine())
                {
                    case "1": AddShip(); break;
                    case "2": AddContainer(); break;
                    case "3": LoadContainerOntoShip(); break;
                    case "4": UnloadContainer(); break;
                    case "5": ListShips(); break;
                    case "6": ListContainers(); break;
                    case "7": ReplaceContainerOnShip(); break;
                    case "8": TransferContainerBetweenShips(); break;
                    case "9": return;
                    default: Console.WriteLine("Niepoprawna opcja, spróbuj ponownie."); break;
                }
                Console.WriteLine("\nNaciśnij dowolny klawisz, aby kontynuować...");
                Console.ReadKey();
            }
        }

        static void AddShip()
        {
            Console.Write("Podaj nazwę statku: ");
            string name = Console.ReadLine();

            Console.Write("Podaj maksymalną prędkość (węzły): ");
            double maxSpeed = double.Parse(Console.ReadLine());

            Console.Write("Podaj maksymalną liczbę kontenerów: ");
            int maxContainers = int.Parse(Console.ReadLine());

            Console.Write("Podaj maksymalną wagę (t): ");
            double maxWeight = double.Parse(Console.ReadLine());

            ships.Add(new Ship(name, maxSpeed, maxContainers, maxWeight));
            Console.WriteLine($"Dodano statek {name}.");
        }

        static void AddContainer()
        {
            Console.WriteLine("Wybierz typ kontenera: (L) Płyn, (G) Gaz, (C) Chłodniczy");
            string type = Console.ReadLine().ToUpper();

            Console.Write("Podaj maksymalną pojemność (kg): ");
            double maxCapacity = double.Parse(Console.ReadLine());

            Console.Write("Podaj wagę własną (kg): ");
            double tareWeight = double.Parse(Console.ReadLine());

            Container container = type switch
            {
                "L" => new LiquidContainer(maxCapacity, tareWeight, AskIfHazardous()),
                "G" => new GasContainer(maxCapacity, tareWeight, AskForPressure()),
                "C" => new RefrigeratedContainer(maxCapacity, tareWeight, AskForTemperature()),
                _ => null
            };

            if (container != null)
            {
                containers.Add(container);
                Console.WriteLine($"Utworzono kontener {container.SerialNumber}.");
            }
            else
            {
                Console.WriteLine("Niepoprawny typ kontenera.");
            }
            
            Console.Write("Podaj wagę załadunku kontenera (kg): ");
            double loadWeight = double.Parse(Console.ReadLine());
            container.SetLoadWeight(loadWeight);
            Console.WriteLine($"Załadunek kontenera {container.SerialNumber} wynosi {loadWeight} kg.");
        }

        static void LoadContainerOntoShip()
        {
            ListShips();
            Console.Write("Podaj nazwę statku, do którego chcesz dodać kontener: ");
            string shipName = Console.ReadLine();

            var ship = ships.FirstOrDefault(s => s.Name == shipName);
            if (ship == null)
            {
                Console.WriteLine("Nie znaleziono statku.");
                return;
            }

            Console.Write("Podaj numer seryjny kontenera: ");
            string serialNumber = Console.ReadLine();

            Container container = FindContainer(serialNumber);
            if (container == null)
            {
                Console.WriteLine("Nie znaleziono kontenera.");
                return;
            }

            ship.AddContainer(container);
        }

        static void UnloadContainer()
        {
            Console.Write("Podaj numer seryjny kontenera do rozładunku: ");
            string serialNumber = Console.ReadLine();

            Container container = FindContainer(serialNumber);
            if (container != null)
            {
                container.Unload();
                Console.WriteLine($"Kontener {serialNumber} został rozładowany.");

                foreach (var ship in ships)
                {
                    if (ship.Containers.Contains(container) && container.CurrentLoad == 0)
                    {
                        ship.RemoveContainer(container.SerialNumber);
                    }
                }
            }
            else
            {
                Console.WriteLine("Nie znaleziono kontenera.");
            }
        }

        static void ReplaceContainerOnShip()
        {
            ListShips();
            Console.Write("Podaj nazwę statku, na którym chcesz zastąpić kontener: ");
            string shipName = Console.ReadLine();

            var ship = ships.FirstOrDefault(s => s.Name == shipName);
            if (ship == null)
            {
                Console.WriteLine("Nie znaleziono statku.");
                return;
            }

            Console.Write("Podaj numer seryjny kontenera do zastąpienia: ");
            string oldSerialNumber = Console.ReadLine();

            Container oldContainer = ship.Containers.FirstOrDefault(c => c.SerialNumber == oldSerialNumber);
            if (oldContainer == null)
            {
                Console.WriteLine("Nie znaleziono kontenera o podanym numerze seryjnym na statku.");
                return;
            }

            Console.Write("Podaj numer seryjny nowego kontenera: ");
            string newSerialNumber = Console.ReadLine();

            Container newContainer = FindContainer(newSerialNumber);
            if (newContainer == null)
            {
                Console.WriteLine("Nie znaleziono nowego kontenera.");
                return;
            }

            ship.RemoveContainer(oldSerialNumber);
            ship.AddContainer(newContainer);
        }

        static void TransferContainerBetweenShips()
        {
            ListShips();
            Console.Write("Podaj nazwę statku, z którego chcesz przenieść kontener: ");
            string sourceShipName = Console.ReadLine();

            var sourceShip = ships.FirstOrDefault(s => s.Name == sourceShipName);
            if (sourceShip == null)
            {
                Console.WriteLine("Nie znaleziono statku źródłowego.");
                return;
            }

            Console.Write("Podaj nazwę statku, na który chcesz przenieść kontener: ");
            string destinationShipName = Console.ReadLine();

            var destinationShip = ships.FirstOrDefault(s => s.Name == destinationShipName);
            if (destinationShip == null)
            {
                Console.WriteLine("Nie znaleziono statku docelowego.");
                return;
            }

            Console.Write("Podaj numer seryjny kontenera do przeniesienia: ");
            string serialNumber = Console.ReadLine();

            Container container = sourceShip.Containers.FirstOrDefault(c => c.SerialNumber == serialNumber);
            if (container == null)
            {
                Console.WriteLine("Nie znaleziono kontenera na statku źródłowym.");
                return;
            }

            sourceShip.RemoveContainer(serialNumber);
            destinationShip.AddContainer(container);
        }

        static void ListShips()
        {
            if (ships.Count == 0)
            {
                Console.WriteLine("Brak statków.");
                return;
            }

            foreach (var ship in ships)
            {
                Console.WriteLine($"Statek: {ship.Name}, Prędkość: {ship.MaxSpeed} węzłów, Kontenery: {ship.Containers.Count}/{ship.MaxContainers}, Waga: {ship.CurrentWeight()}t/{ship.MaxWeight}t");
            }
        }

        static void ListContainers()
        {
            foreach (var container in containers)
            {
                Console.WriteLine($"Kontener {container.SerialNumber} typu {container.Type}, Ładunek: {container.CurrentLoad} kg, Załadunek: {container.LoadWeight} kg, Waga: {container.TareWeight} kg");
            }
        }

        static Container FindContainer(string serialNumber)
        {
            return containers.FirstOrDefault(c => c.SerialNumber == serialNumber);
        }

        static bool AskIfHazardous()
        {
            Console.Write("Czy kontener jest niebezpieczny? (T/N): ");
            return Console.ReadLine().ToUpper() == "T";
        }

        static double AskForPressure()
        {
            Console.Write("Podaj ciśnienie gazu (w barach): ");
            return double.Parse(Console.ReadLine());
        }

        static double AskForTemperature()
        {
            Console.Write("Podaj temperaturę (w °C): ");
            return double.Parse(Console.ReadLine());
        }
    }
}
