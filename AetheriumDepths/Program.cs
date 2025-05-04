using System;
using System.IO;

// Redirect console output to a log file for debugging
using var logFile = new StreamWriter("game_log.txt");
Console.SetOut(logFile);
Console.WriteLine("Starting Aetherium Depths...");

try
{
    using var game = new AetheriumDepths.AetheriumGame();
    game.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"EXCEPTION: {ex.Message}");
    Console.WriteLine($"STACKTRACE: {ex.StackTrace}");
}

Console.WriteLine("Game exited.");
