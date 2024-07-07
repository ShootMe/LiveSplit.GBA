using LiveSplit.Memory;
using System;
using System.Diagnostics;
namespace LiveSplit.GBA {
    public partial class SplitterMemory {
        public Process Program { get; set; }
        public bool IsHooked { get; set; } = false;
        private DateTime lastHooked;
        private IntPtr emuPtr;
        private DateTime emuLastCheck;
        public SplitterMemory() {
            lastHooked = DateTime.MinValue;
            emuLastCheck = DateTime.MinValue;
        }
        public string Pointer() {
            return SetEmulator() ? emuPtr.ToString("X") : string.Empty;
        }
        private bool SetEmulator() {
            if (emuLastCheck > DateTime.Now) { return emuPtr != IntPtr.Zero; }
            emuLastCheck = DateTime.Now.AddSeconds(1);
            Module64 mgba = Program.Module64("mgba.dll");
            if (mgba == null) { return false; }

            MemorySearcher searcher = new MemorySearcher();
            searcher.MemoryFilter = delegate (MemInfo info) {
                return (long)info.RegionSize == 0x48000 && (info.AllocationProtect & 0x4) != 0;
            };
            emuPtr = searcher.FilterMemory(Program);

            return emuPtr != IntPtr.Zero;
        }
        public T Read<T>(RAMSection section, uint address) where T : unmanaged {
            if (SetEmulator()) {
                switch (section) {
                    case RAMSection.IWRAM: return Program.Read<T>(emuPtr + 0x40000, (int)address);
                    case RAMSection.EWRAM: return Program.Read<T>(emuPtr, (int)address);
                }
            }
            return default(T);
        }
        public bool HookProcess() {
            IsHooked = Program != null && !Program.HasExited;
            if (!IsHooked && DateTime.Now > lastHooked.AddSeconds(1)) {
                lastHooked = DateTime.Now;
                emuPtr = IntPtr.Zero;
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