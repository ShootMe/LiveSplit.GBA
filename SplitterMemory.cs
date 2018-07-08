using LiveSplit.Memory;
using System;
using System.Diagnostics;
namespace LiveSplit.GBA {
	public partial class SplitterMemory {
		private static ProgramPointer RAM = new ProgramPointer(AutoDeref.None, 0);
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public SplitterMemory() {
			lastHooked = DateTime.MinValue;
		}
		public string Pointer() {
			return RAM.GetPointer(Program).ToString("X");
		}
		public T Read<T>(RAMSection section, uint address) where T : struct {
			switch (section) {
				case RAMSection.IWRAM: return RAM.Read<T>(Program, 0x0, 0x38, 0x28, (int)address);
				case RAMSection.EWRAM: return RAM.Read<T>(Program, 0x0, 0x40, 0x28, (int)address);
				case RAMSection.BIOS: return RAM.Read<T>(Program, 0x0, 0x48, 0x28, (int)address);
				case RAMSection.PALRAM: return RAM.Read<T>(Program, 0x0, 0x50, 0x28, (int)address);
				case RAMSection.VRAM: return RAM.Read<T>(Program, 0x0, 0x58, 0x28, (int)address);
				case RAMSection.OAM: return RAM.Read<T>(Program, 0x0, 0x60, 0x28, (int)address);
				case RAMSection.ROM: return RAM.Read<T>(Program, 0x0, 0x68, 0x28, (int)address);
				case RAMSection.SRAM: return RAM.Read<T>(Program, 0x0, 0x70, 0x28, (int)address);
				case RAMSection.CWRAM: return RAM.Read<T>(Program, 0x0, 0x78, 0x28, (int)address);
			}
			return default(T);
		}
		public bool HookProcess() {
			IsHooked = Program != null && !Program.HasExited;
			if (!IsHooked && DateTime.Now > lastHooked.AddSeconds(1)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcesses();
				Program = null;
				for (int i = 0; i < processes.Length; i++) {
					Process process = processes[i];
					if (process.ProcessName.Equals("EmuHawk", StringComparison.OrdinalIgnoreCase)) {
						Program = process;
						break;
					}
				}

				if (Program != null && !Program.HasExited) {
					MemoryReader.Update64Bit(Program);
					IsHooked = true;
				}
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) {
				Program.Dispose();
			}
		}
	}
}