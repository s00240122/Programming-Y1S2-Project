﻿using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace Csharp_console_app
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"../../../../../testfile.csv";

            List<Passenger> passengerList = GetPassengerData(filePath);


            Menu mainMenu = new Menu(
                new MenuOption("Ship reports", () => ShipReports(passengerList)),
                new MenuOption("Occupation report", () => OccupationReport(passengerList)),
                new MenuOption("Age report", () => AgeReport(passengerList))
                );

            mainMenu.Run();

        }

        static void ShipReports(List<Passenger> passengers)
        {
            List<Ship> ships = passengers.GetShips();

            MenuOption[] menuOptions = ships
                .Select(ship => new MenuOption($"{ship.ShipID}", () => ShowShipInfoAndPassengers(ship, passengers)))
                .ToArray();

            Menu subMenu = new Menu(
                withExit: false,
                menuOptions
                );

            Console.WriteLine("The available ships are: ");

            subMenu.Run();
        }

        static void ShowShipInfoAndPassengers(Ship ship, List<Passenger> passengers)
        {
            List<Passenger> passengersOnShip = passengers.GetPassengersOnShip(ship);
            
            Console.WriteLine($"{ship.ShipID}: from {ship.DepartureSeaport} to {ship.DestinationCountry} with {passengersOnShip.Count} passengers, arrives {ship.ArrivalDate}");
            
            foreach (Passenger passenger in passengersOnShip)
                Console.WriteLine(passenger.ToString());
        }


        static void ShipReports2(List<Passenger> passengers)
        {
            List<Ship> ships = passengers.GetShips();

            string choice = "";

            Console.WriteLine("The available ships are:");
            for (int i = 0; i < ships.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {ships[i].ShipID}");
            }

            Console.Write("Pick one: ");
            choice = Console.ReadLine();

            // insert data validation and stuff

            Ship ship = ships[int.Parse(choice) - 1];
            List<Passenger> passengersOnShip = passengers.GetPassengersOnShip(ship);

            Console.WriteLine($"{ship.ShipID}: from {ship.DepartureSeaport} to {ship.DestinationCountry} with {passengersOnShip.Count} passengers");
            foreach(Passenger passenger in passengersOnShip)
                Console.WriteLine(passenger.ToString());
        }

        static void OccupationReport(List<Passenger> passengers)
        {
            List<(string, int)> occupations = passengers.GetOccupationsAndAmounts();

            Console.WriteLine($"{"Occupation", -46} Amount");
            foreach (var item in occupations)
            {
                var (occupation, count) = item;
                Console.WriteLine($"{occupation, -46} {count}");
            }
        }

        static void AgeReport(List<Passenger> passengers)
        {
            // this ageGroups list could be in global
            // or maybe not
            // idk

            List<(string, int)> ageGroups = new List<(string, int)>()
            { 
                // the number represents the lower bound for your age group
                ("Infant", 0),
                ("Child", 1),
                ("Teen", 12),
                ("Young Adult", 20),
                ("Adult", 30),
                ("Older Adult", 50)
            };

            List<(string, int, int)> ageGroupAmounts = passengers.GetAgeGroupAmounts(ageGroups);

            foreach (var item in ageGroupAmounts)
            {
                var (name, age, amount) = item;

                string nameAgeTemp = $"{name} (>{age})";

                if (age != Global.UNKNOWN_VALUE)
                    Console.WriteLine($"{nameAgeTemp, -20}: {amount}");
                else
                    Console.WriteLine($"{name, -20}: {amount}");
            }
        }


        static List<Passenger> GetPassengerData(string path)
        {
            List<Passenger> passengers = new List<Passenger>();
            try
            {
                using (StreamReader sr = File.OpenText(path))
                {
                    // read and discard the first line
                    sr.ReadLine();

                    string? s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] fields = s.Split(',', StringSplitOptions.RemoveEmptyEntries);

                        if (fields.Length < 10)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine($"[Warning] Line '{s}' has fewer than 10 non-empty fields. Skipping.");
                            Console.ResetColor();
                            continue;
                        }

                        string dateString = fields[9];
                        DateOnly date;

                        if (!DateOnly.TryParse(dateString, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out date))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine($"[Warning] Line '{s}' has an incorrect date format ({dateString}). Skipping.");
                            Console.ResetColor();
                            continue;
                        }


                        Passenger passenger = new()  // wack syntax
                        {
                            // the current implementation depends on the correct ordering of the columns
                            // it's possible to make it depend on the header row names
                            // but I'm going with the former, since changing the header row is easier than rearranging all the columns

                            LastName = fields[0],
                            FirstName = fields[1],
                            Age = Passenger.AgeParse(fields[2]),
                            Sex = fields[3],
                            Occupation = fields[4],
                            NativeCountry = fields[5],
                            Ship = new Ship(
                                fields[6],
                                fields[7],
                                fields[8],
                                date
                                )
                        };

                        passengers.Add(passenger);

                        // debug stuff
                        // Console.WriteLine(passenger.ToString());
                        Console.WriteLine(s);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("File loaded successfully!");
                Console.ResetColor();
            }
            catch (Exception ex) when (
                ex is DirectoryNotFoundException ||
                ex is FileNotFoundException ||
                ex is UnauthorizedAccessException ||
                ex is IOException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.ResetColor();

                Environment.Exit(0);
            }

            return passengers;
        }




    }
}


/*
List<Ship> ships = passengerList.GetShips();

foreach (Ship ship in ships)
    Console.WriteLine($"{ship.ShipID} arrived on {ship.ArrivalDate} WITH COUNT {ship.Count}");


List<Passenger> filtered = passengerList.GetPassengersOnShip(ships[0]);

Console.WriteLine($"{ships[0].ShipID} leaving from {ships[0].DepartureSeaport}\nArrived: {ships[0].ArrivalDate} with {filtered.Count} passengers");

foreach(Passenger passenger in filtered)
    Console.WriteLine(passenger.ToString());

Console.WriteLine(filtered.Count);





List<(string, int)> occupations = passengerList.GetOccupationsAndAmounts();

foreach(var item in occupations)
{
    var (occupation, count) = item;
    Console.WriteLine($"uhh {occupation} and {count}");
}





List<(string, int)> ageGroups = new List<(string, int)>() 
{ 
    // the number represents the lower bound for your age group
    ("Infant", 0), 
    ("Child", 1), 
    ("Teen", 12), 
    ("Young Adult", 20),
    ("Adult", 30),
    ("Older Adult", 50)
};

List<(string, int, int)> ageGroupAmounts = passengerList.GetAgeGroupAmounts(ageGroups);

foreach(var item in ageGroupAmounts)
{
    var (name, age, amount) = item;
    if (age != Global.UNKNOWN_VALUE)
        Console.WriteLine($"{name} >{age}: {amount}");
    else
        Console.WriteLine($"{name}: {amount}");
}

*/