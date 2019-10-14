// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
namespace HackF5.UnitySpy.Detail
{
    internal static class Offsets
    {
        public const uint ImageDosHeader_e_lfanew = 0x3c;

        public const uint ImageExportDirectory_AddressOfFunctions = 0x1c;

        public const uint ImageExportDirectory_AddressOfNames = 0x20;

        public const uint ImageExportDirectory_NumberOfFunctions = 0x14;

        public const uint ImageNTHeaders_ExportDirectoryAddress = 0x78;

        public const uint ImageNTHeaders_Machine = 0x4;

        public const uint ImageNTHeaders_Signature = 0x0;

        public const uint MonoAssembly_image = 0x40;

        public const uint MonoAssembly_name = 0x8;

        public const uint MonoAssembly_sizeof = 0x54;

        public const uint MonoClass_bitfields = 0x14;

        public const uint MonoClass_byval_arg = 0x8c;

        public const uint MonoClass_field_count = 0x68;

        public const uint MonoClass_fields = 0x78;

        public const uint MonoClass_name = 0x34;

        public const uint MonoClass_name_space = 0x38;

        public const uint MonoClass_nested_in = 0x28;

        public const uint MonoClass_next_class_cache = 0xac;

        public const uint MonoClass_parent = 0x24;

        public const uint MonoClass_runtime_info = 0xa8;

        public const uint MonoClass_sizes = 0x5c;

        public const uint MonoClassField_name = 0x4;

        public const uint MonoClassField_offset = 0xc;

        public const uint MonoClassField_parent = 0x8;

        public const uint MonoClassField_sizeof = 0x10;

        public const uint MonoClassField_type = 0x0;

        public const uint MonoClassRuntimeInfo_domain_vtables = 0x4;

        public const uint MonoDomain_domain_assemblies = 0x70;

        public const uint MonoDomain_sizeof = 0x144;

        public const uint MonoImage_class_cache = 0x2a0;

        public const uint MonoInternalHashTable_size = 0xc;

        public const uint MonoInternalHashTable_table = 0x14;

        public const uint MonoType_attrs = 0x4;

        public const uint MonoType_sizeof = 0x8;

        public const uint MonoVTable_data = 0xc;
    }
}