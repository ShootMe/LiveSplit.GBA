using LiveSplit.Memory;
using System;
using System.Diagnostics;
namespace LiveSplit.GBA {
	public enum PointerVersion {
		None,
		Win10_221,
		Win7_221,
		Win7_230
	}
	public enum AutoDeref {
		None,
		Single,
		Double
	}
	public class ProgramSignature {
		public PointerVersion Version { get; set; }
		public string Signature { get; set; }
		public int Offset { get; set; }
		public ProgramSignature(PointerVersion version, string signature, int offset) {
			Version = version;
			Signature = signature;
			Offset = offset;
		}
		public override string ToString() {
			return Version.ToString() + " - " + Signature;
		}
	}
	public class ProgramPointer {
		private int lastID;
		private DateTime lastTry;
		private ProgramSignature[] signatures;
		private int[] offsets;
		public IntPtr Pointer { get; private set; }
		public PointerVersion Version { get; private set; }
		public AutoDeref AutoDeref { get; private set; }

		public ProgramPointer(AutoDeref autoDeref, params ProgramSignature[] signatures) {
			AutoDeref = autoDeref;
			this.signatures = signatures;
			lastID = -1;
			lastTry = DateTime.MinValue;
		}
		public ProgramPointer(AutoDeref autoDeref, params int[] offsets) {
			AutoDeref = autoDeref;
			this.offsets = offsets;
			lastID = -1;
			lastTry = DateTime.MinValue;
		}

		public T Read<T>(Process program, params int[] offsets) where T : struct {
			GetPointer(program);
			return program.Read<T>(Pointer, offsets);
		}
		public byte[] ReadBytes(Process program, int length, params int[] offsets) {
			GetPointer(program);
			return program.Read(Pointer, length, offsets);
		}
		public void Write<T>(Process program, T value, params int[] offsets) where T : struct {
			GetPointer(program);
			program.Write<T>(Pointer, value, offsets);
		}
		public void Write(Process program, byte[] value, params int[] offsets) {
			GetPointer(program);
			program.Write(Pointer, value, offsets);
		}
		public void ClearPointer() {
			Pointer = IntPtr.Zero;
		}
		public IntPtr GetPointer(Process program) {
			if (program == null) {
				Pointer = IntPtr.Zero;
				lastID = -1;
				return Pointer;
			} else if (program.Id != lastID) {
				Pointer = IntPtr.Zero;
				lastID = program.Id;
			}

			if (Pointer == IntPtr.Zero && DateTime.Now > lastTry.AddSeconds(1)) {
				lastTry = DateTime.Now;

				Pointer = GetVersionedFunctionPointer(program);
				if (Pointer != IntPtr.Zero) {
					if (AutoDeref != AutoDeref.None) {
						if (MemoryReader.is64Bit && Version == PointerVersion.Win10_221) {
							Pointer = (IntPtr)program.Read<ulong>(Pointer);
						} else {
							Pointer = (IntPtr)program.Read<uint>(Pointer);
						}
						if (AutoDeref == AutoDeref.Double) {
							if (MemoryReader.is64Bit && Version == PointerVersion.Win10_221) {
								Pointer = (IntPtr)program.Read<ulong>(Pointer);
							} else {
								Pointer = (IntPtr)program.Read<uint>(Pointer);
							}
						}
					}
				}
			}
			return Pointer;
		}
		private IntPtr GetVersionedFunctionPointer(Process program) {
			MemorySearcher searcher = new MemorySearcher();
			searcher.MemoryFilter = delegate (MemInfo info) {
				return (info.Protect & 0x40) != 0 && (info.State & 0x1000) != 0 && (info.Type & 0x20000) != 0;
			};
			//BizHawk.Client.Common.Global.get_SystemInfo
			ProgramSignature signature = new ProgramSignature(PointerVersion.Win7_221, "488B00E9????????BA????????488B1248B9????????????????E8????????488BC849BB", 9);
			IntPtr ptr = searcher.FindSignature(program, signature.Signature);

			if (ptr == IntPtr.Zero) {
				signature = new ProgramSignature(PointerVersion.Win7_230, "488BF8BA????????488B124885D20F84", 4);
				ptr = searcher.FindSignature(program, signature.Signature);
			}
			if (ptr == IntPtr.Zero) {
				signature = new ProgramSignature(PointerVersion.Win10_221, "83EC2048B9????????????????488B0949BB????????????????390941FF13488BF0488BCEE8", 5);
				ptr = searcher.FindSignature(program, signature.Signature);
			}

			if (ptr != IntPtr.Zero) {
				AutoDeref = AutoDeref.Single;
				Version = signature.Version;
				return ptr + signature.Offset;
			}
			Version = PointerVersion.None;
			return IntPtr.Zero;
		}
	}
}