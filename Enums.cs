using System.ComponentModel;
namespace LiveSplit.GBA {
	public enum SplitType {
		[Description("Equals")]
		Equals,
		[Description("Less Than")]
		LessThan,
		[Description("Greater Than")]
		GreaterThan,
		[Description("Changed")]
		Changed,
		[Description("Changed From")]
		ChangedFrom,
	}
	public enum ValueSize {
		[Description("8 U-Bits")]
		UInt8,
		[Description("16 U-Bits")]
		UInt16,
		[Description("32 U-Bits")]
		UInt32,
		[Description("8 S-Bits")]
		Int8,
		[Description("16 S-Bits")]
		Int16,
		[Description("32 S-Bits")]
		Int32,
		[Description("Float")]
		Float,
		[Description("Manual Split")]
		Manual,
	}
	public enum RAMSection {
		[Description("IWRAM")]
		IWRAM,
		[Description("EWRAM")]
		EWRAM,
		[Description("CWRAM")]
		CWRAM,
		[Description("PALRAM")]
		PALRAM,
		[Description("VRAM")]
		VRAM,
		[Description("SRAM")]
		SRAM,
		[Description("OAM")]
		OAM,
		[Description("ROM")]
		ROM,
		[Description("BIOS")]
		BIOS,
	}
}