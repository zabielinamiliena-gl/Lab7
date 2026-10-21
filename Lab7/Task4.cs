using System;

namespace Variant1_SmartHomeDevices
{
    public interface IPowerControl
    {
        void PowerOn();
        void PowerOff();
    }

    public interface ITemperatureControl
    {
        void SetTemperature(double temp);
    }

    public interface IBrightnessControl
    {
        void SetBrightness(int level);
    }

    public interface ICoffeeBrewing
    {
        void BrewCoffee();
    }

    public class SmartThermostat : IPowerControl, ITemperatureControl
    {
        public void PowerOn() => Console.WriteLine("Thermostat powered on.");
        public void PowerOff() => Console.WriteLine("Thermostat powered off.");
        public void SetTemperature(double temp) => Console.WriteLine($"Temperature set to {temp}°C.");
    }

    public class SmartLight : IPowerControl, IBrightnessControl
    {
        public void PowerOn() => Console.WriteLine("Light turned on.");
        public void PowerOff() => Console.WriteLine("Light turned off.");
        public void SetBrightness(int level) => Console.WriteLine($"Brightness set to {level}%.");
    }

    public class SmartCoffeeMaker : IPowerControl, ICoffeeBrewing
    {
        public void PowerOn() => Console.WriteLine("Coffee maker powered on.");
        public void PowerOff() => Console.WriteLine("Coffee maker powered off.");
        public void BrewCoffee() => Console.WriteLine("Brewing fresh coffee...");
    }

    public class PowerClient
    {
        public void Run(IPowerControl device)
        {
            device.PowerOn();
            device.PowerOff();
        }
    }

    public class ClimateClient
    {
        public void Run(ITemperatureControl device)
        {
            device.SetTemperature(22);
        }
    }

    public class LightingClient
    {
        public void Run(IBrightnessControl device)
        {
            device.SetBrightness(70);
        }
    }

    public class KitchenClient
    {
        public void Run(ICoffeeBrewing device)
        {
            device.BrewCoffee();
        }
    }

    class Program
    {
        static void Main()
        {
            SmartThermostat thermostat = new SmartThermostat();
            SmartLight light = new SmartLight();
            SmartCoffeeMaker coffeeMaker = new SmartCoffeeMaker();

            PowerClient power = new PowerClient();
            power.Run(thermostat);
            power.Run(light);
            power.Run(coffeeMaker);

            new ClimateClient().Run(thermostat);
            new LightingClient().Run(light);
            new KitchenClient().Run(coffeeMaker);
        }
    }
}
