namespace HackF5.UnitySpy.Detail
{    class Offsets
    {
        public static uint ImageDosHeader_e_lfanew = 0x3c;
        public static uint ImageNTHeaders_Signature = 0x0;
        public static uint ImageNTHeaders_Machine = 0x4;
        public static uint ImageNTHeaders_ExportDirectoryAddress = 0x78;
        public static uint ImageExportDirectory_NumberOfFunctions = 0x14;
        public static uint ImageExportDirectory_AddressOfFunctions = 0x1c;
        public static uint ImageExportDirectory_AddressOfNames = 0x20;
        public static uint MonoDomain_sizeof = 0x144;
        public static uint MonoDomain_domain_assemblies = 0x70;
        public static uint MonoAssembly_sizeof = 0x54;
        public static uint MonoAssembly_name = 0x8;
        public static uint MonoAssembly_image = 0x40;
        public static uint MonoImage_class_cache = 0x2a0;
        public static uint MonoInternalHashTable_size = 0xc;
        public static uint MonoInternalHashTable_table = 0x14;
        public static uint MonoClass_parent = 0x24;
        public static uint MonoClass_nested_in = 0x28;
        public static uint MonoClass_runtime_info = 0xa8;
        public static uint MonoClass_name = 0x34;
        public static uint MonoClass_name_space = 0x38;
        public static uint MonoClass_next_class_cache = 0xac;
        public static uint MonoClass_fields = 0x78;
        public static uint MonoClass_sizes = 0x5c;
        public static uint MonoClass_byval_arg = 0x8c;
        public static uint MonoClass_bitfields = 0x14;
        public static uint MonoClass_field_count = 0x68;
        public static uint MonoClassField_sizeof = 0x10;
        public static uint MonoClassField_type = 0x0;
        public static uint MonoClassField_name = 0x4;
        public static uint MonoClassField_parent = 0x8;
        public static uint MonoClassField_offset = 0xc;
        public static uint MonoType_attrs = 0x4;
        public static uint MonoType_sizeof = 0x8;
        public static uint MonoClassRuntimeInfo_domain_vtables = 0x4;
        public static uint MonoVTable_data = 0xc;
    }
}
