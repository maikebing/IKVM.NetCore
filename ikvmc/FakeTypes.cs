﻿/*
  Copyright (C) 2008 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace IKVM.Internal
{
	static class FakeTypes
	{
		private static Type genericEnumEnumType;
		private static Type genericDelegateInterfaceType;
		private static Type genericAttributeAnnotationType;
		private static Type genericAttributeAnnotationMultipleType;
		private static Type genericAttributeAnnotationReturnValueType;

		internal static Type GetEnumType(Type enumType)
		{
			return genericEnumEnumType.MakeGenericType(enumType);
		}

		internal static Type GetDelegateType(Type delegateType)
		{
			return genericDelegateInterfaceType.MakeGenericType(delegateType);
		}

		internal static Type GetAttributeType(Type attributeType)
		{
			return genericAttributeAnnotationType.MakeGenericType(attributeType);
		}

		internal static Type GetAttributeMultipleType(Type attributeType)
		{
			return genericAttributeAnnotationMultipleType.MakeGenericType(attributeType);
		}

		internal static Type GetAttributeReturnValueType(Type attributeType)
		{
			return genericAttributeAnnotationReturnValueType.MakeGenericType(attributeType);
		}

		internal static void CreatePre(ModuleBuilder modb)
		{
			TypeBuilder tb = modb.DefineType(DotNetTypeWrapper.GenericDelegateInterfaceTypeName, TypeAttributes.Interface | TypeAttributes.Abstract | TypeAttributes.Public);
			tb.DefineGenericParameters("T")[0].SetBaseTypeConstraint(typeof(MulticastDelegate));
			genericDelegateInterfaceType = tb.CreateType();
		}

		internal static void Create(ModuleBuilder modb, ClassLoaderWrapper loader)
		{
			CreateEnumEnum(modb, loader);

			TypeWrapper annotationTypeWrapper = loader.LoadClassByDottedName("java.lang.annotation.Annotation");
			annotationTypeWrapper.Finish();
			genericAttributeAnnotationType = CreateAnnotationType(modb, DotNetTypeWrapper.GenericAttributeAnnotationTypeName, annotationTypeWrapper);
			genericAttributeAnnotationMultipleType = CreateAnnotationType(modb, DotNetTypeWrapper.GenericAttributeAnnotationMultipleTypeName, annotationTypeWrapper);
			genericAttributeAnnotationReturnValueType = CreateAnnotationType(modb, DotNetTypeWrapper.GenericAttributeAnnotationReturnValueTypeName, annotationTypeWrapper);
		}

		private static void CreateEnumEnum(ModuleBuilder modb, ClassLoaderWrapper loader)
		{
			TypeWrapper enumTypeWrapper = loader.LoadClassByDottedName("java.lang.Enum");
			enumTypeWrapper.Finish();
			TypeBuilder tb = modb.DefineType(DotNetTypeWrapper.GenericEnumEnumTypeName, TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public, enumTypeWrapper.TypeAsBaseType);
			GenericTypeParameterBuilder gtpb = tb.DefineGenericParameters("T")[0];
			gtpb.SetBaseTypeConstraint(typeof(Enum));
			CountingILGenerator ilgen = tb.DefineConstructor(MethodAttributes.Private, CallingConventions.Standard, new Type[] { typeof(string), typeof(int) }).GetILGenerator();
			ilgen.Emit(OpCodes.Ldarg_0);
			ilgen.Emit(OpCodes.Ldarg_1);
			ilgen.Emit(OpCodes.Ldarg_2);
			enumTypeWrapper.GetMethodWrapper("<init>", "(Ljava.lang.String;I)V", false).EmitCall(ilgen);
			ilgen.Emit(OpCodes.Ret);
			genericEnumEnumType = tb.CreateType();
		}

		private static Type CreateAnnotationType(ModuleBuilder modb, string name, TypeWrapper annotationTypeWrapper)
		{
			TypeBuilder tb = modb.DefineType(name, TypeAttributes.Interface | TypeAttributes.Abstract | TypeAttributes.Public);
			tb.DefineGenericParameters("T")[0].SetBaseTypeConstraint(typeof(Attribute));
			return tb.CreateType();
		}

		internal static void Load(Assembly assembly)
		{
			genericEnumEnumType = assembly.GetType(DotNetTypeWrapper.GenericEnumEnumTypeName);
			genericDelegateInterfaceType = assembly.GetType(DotNetTypeWrapper.GenericDelegateInterfaceTypeName);
			genericAttributeAnnotationType = assembly.GetType(DotNetTypeWrapper.GenericAttributeAnnotationTypeName);
			genericAttributeAnnotationMultipleType = assembly.GetType(DotNetTypeWrapper.GenericAttributeAnnotationMultipleTypeName);
			genericAttributeAnnotationReturnValueType = assembly.GetType(DotNetTypeWrapper.GenericAttributeAnnotationReturnValueTypeName);
		}
	}
}