﻿using Discord.Commands;
using System.Reflection;

namespace ExampleBot.Core.Managers
{
    public static class CommandManager
    {
        private static CommandService _commandService = ServiceManager.GetService<CommandService>();
        public static async Task LoadCommandsAsync()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceManager.Provider);
            foreach (var command in _commandService.Commands)
                Console.WriteLine($"Command {command.Name} was loaded."); // Logs
        }
    }
}
