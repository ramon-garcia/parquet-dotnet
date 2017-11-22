﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using Parquet.Data;

namespace Parquet.File
{
   static class TypeExtensions
   {
      public static IList CreateGenericList(this DataField df, IEnumerable values)
      {
         Type elementType = df.ClrType;

         //make the type nullable if it's not a class
         if(df.HasNulls && !df.IsArray)
         {
            if(!elementType.GetTypeInfo().IsClass)
            {
               elementType = typeof(Nullable<>).MakeGenericType(elementType);
            }
         }

         //create generic list instance
         Type listType = typeof(List<>);
         Type listGType = listType.MakeGenericType(elementType);

         IList result = (IList)Activator.CreateInstance(listGType);

         foreach(object value in values)
         {
            result.Add(value);
         }

         return result;
      }

      public static bool TryExtractEnumerableType(this Type t, out Type baseType)
      {
         TypeInfo ti = t.GetTypeInfo();

         if(ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>))
         {
            baseType = ti.GenericTypeArguments[0];
            return true;
         }

         if(ti.IsArray)
         {
            baseType = ti.GetElementType();
            return true;
         }

         baseType = null;
         return false;
      }

      public static bool TryExtractDictionaryType(this Type t, out Type keyType, out Type valueType)
      {
         TypeInfo ti = t.GetTypeInfo();

         if(ti.IsGenericType && ti.GetGenericTypeDefinition().GetTypeInfo().IsAssignableFrom(typeof(Dictionary<,>).GetTypeInfo()))
         {
            keyType = ti.GenericTypeArguments[0];
            valueType = ti.GenericTypeArguments[1];
            return true;
         }

         keyType = valueType = null;
         return false;
      }

      public static bool IsNullable(this IList list)
      {
         TypeInfo ti = list.GetType().GetTypeInfo();

         Type t = ti.GenericTypeArguments[0];
         Type gt = t.GetTypeInfo().IsGenericType ? t.GetTypeInfo().GetGenericTypeDefinition() : null;

         return gt == typeof(Nullable<>) || t.GetTypeInfo().IsClass;
      }

      public static bool IsNullable(this Type t)
      {
         TypeInfo ti = t.GetTypeInfo();

         return
            ti.IsClass ||
            (ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(Nullable<>));
      }

      public static Type GetNonNullable(this Type t)
      {
         TypeInfo ti = t.GetTypeInfo();

         if(ti.IsClass)
         {
            return t;
         }
         else
         {
            return ti.GenericTypeArguments[0];
         }
      }
   }
}