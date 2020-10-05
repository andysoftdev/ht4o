/** -*- C# -*-
 * Copyright (C) 2010-2016 Thalmann Software & Consulting, http://www.softdev.ch
 *
 * This file is part of ht4o.
 *
 * ht4o is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or any later version.
 *
 * Hypertable is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */

namespace Hypertable.Persistence.Reflection
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization;
    using Hypertable.Persistence.Extensions;

    /// <summary>
    ///     The delegate factory.
    /// </summary>
    internal static class DelegateFactory
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Create an action for the method info specified.
        /// </summary>
        /// <param name="methodInfo">
        ///     The method info.
        /// </param>
        /// <returns>
        ///     The action created or null.
        /// </returns>
        public static Action<object, object> CreateAction(MethodInfo methodInfo)
        {
            if (methodInfo == null || methodInfo.GetParameters().Length != 1 || methodInfo.ReturnType != typeof(void) ||
                methodInfo.DeclaringType == null)
            {
                return null;
            }

            var method = new DynamicMethod(
                "ht4o_Action" + methodInfo.Name,
                typeof(void),
                new[] { typeof(object), typeof(object) },
                methodInfo.Module,
                true) { InitLocals = false };

            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (methodInfo.DeclaringType.IsValueType) {
                generator.Emit(OpCodes.Unbox, methodInfo.DeclaringType);
            }

            generator.Emit(OpCodes.Ldarg_1);
            if (methodInfo.GetParameters()[0].ParameterType.IsValueType) {
                generator.Emit(OpCodes.Unbox_Any, methodInfo.GetParameters()[0].ParameterType);
            }

            generator.Emit(OpCodes.Call, methodInfo);
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        ///     Returns an IL-compiled function that creates instances of <paramref name="instanceType" /> using its parameter-less
        ///     constructor.
        /// </summary>
        /// <param name="instanceType">
        ///     Type of the instance.
        /// </param>
        /// <returns>
        ///     The function.
        /// </returns>
        public static Func<object> CreateInstance(Type instanceType) {
            if (instanceType == null) {
                throw new ArgumentNullException(nameof(instanceType));
            }

            if (instanceType.IsAbstract || instanceType.IsInterface) {
                return null;
            }

            var method = new DynamicMethod(
                "ht4o_CreateInstance" + instanceType.Name,
                typeof(object),
                Type.EmptyTypes,
                typeof(DelegateFactory).Module,
                true) { InitLocals = false };

            var generator = method.GetILGenerator();

            if (!instanceType.IsValueType) {
                var constructor = instanceType.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null);

                if (constructor != null) {
                    generator.Emit(OpCodes.Newobj, constructor);
                }
                else {
                    generator.Emit(OpCodes.Ldtoken, instanceType);
                    generator.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle)));
                    generator.Emit(OpCodes.Call, typeof(FormatterServices).GetMethod(nameof(FormatterServices.GetUninitializedObject)));
                }
            }
            else {
                var instanceTypeLoc = generator.DeclareLocal(instanceType);
                generator.Emit(OpCodes.Ldloca_S, instanceTypeLoc);
                generator.Emit(OpCodes.Initobj, instanceType);
                generator.Emit(OpCodes.Ldloca_S, instanceTypeLoc);
                generator.Emit(OpCodes.Box, instanceType);
            }

            generator.Emit(OpCodes.Ret);

            return (Func<object>)method.CreateDelegate(typeof(Func<object>));
        }

        /// <summary>
        ///     Returns an IL-compiled function that creates instances of <paramref name="instanceType" /> using its parameter-less
        ///     constructor.
        /// </summary>
        /// <param name="instanceType">
        ///     Type of the instance.
        /// </param>
        /// <returns>
        ///     The function.
        /// </returns>
        public static Func<object> CreateConstructor(Type instanceType)
        {
            if (instanceType == null)
            {
                throw new ArgumentNullException(nameof(instanceType));
            }

            if (instanceType.IsAbstract || instanceType.IsInterface)
            {
                return null;
            }

            if (instanceType.IsValueType())
            {
                var defaultExpression =
                    Expression.Lambda<Func<object>>(
                        Expression.Convert(Expression.Default(instanceType), typeof(object)));
                return defaultExpression.Compile();
            }

            var ctor = instanceType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);

            if (ctor == null)
            {
                return null;
            }

            var factoryExpression = Expression.Lambda<Func<object>>(Expression.New(ctor));
            return factoryExpression.Compile();
        }

        /// <summary>
        ///     Creates a function for the method info specified.
        /// </summary>
        /// <param name="methodInfo">
        ///     The method info.
        /// </param>
        /// <returns>
        ///     The function created or null.
        /// </returns>
        public static Func<object, object, object> CreateFunc(MethodInfo methodInfo)
        {
            if (methodInfo == null || methodInfo.GetParameters().Length != 1 || methodInfo.ReturnType == typeof(void) ||
                methodInfo.DeclaringType == null)
            {
                return null;
            }

            var method = new DynamicMethod(
               "ht4o_Func" + methodInfo.Name,
               typeof(object),
               new[] { typeof(object), typeof(object) },
               methodInfo.Module,
               true) { InitLocals = false };

            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (methodInfo.DeclaringType.IsValueType) {
                generator.Emit(OpCodes.Unbox, methodInfo.DeclaringType);
            }

            generator.Emit(OpCodes.Ldarg_1);
            if (methodInfo.GetParameters()[0].ParameterType.IsValueType) {
                generator.Emit(OpCodes.Unbox_Any, methodInfo.GetParameters()[0].ParameterType);
            }

            generator.Emit(OpCodes.Call, methodInfo);
            if (methodInfo.ReturnType.IsValueType) {
                generator.Emit(OpCodes.Box, methodInfo.ReturnType);
            }

            generator.Emit(OpCodes.Ret);

            return (Func<object, object, object>)method.CreateDelegate(typeof(Func<object, object, object>));
        }

        /// <summary>
        ///     Creates a getter function for the property info specified.
        /// </summary>
        /// <param name="propertyInfo">
        ///     The property info.
        /// </param>
        /// <returns>
        ///     The getter function created or null.
        /// </returns>
        public static Func<object, object> CreateGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || propertyInfo.DeclaringType == null)
            {
                return null;
            }

            if (propertyInfo.GetGetMethod(true) == null) {
                throw new ArgumentException($"Property {propertyInfo.Name} has no getter");
            }

            var method = new DynamicMethod(
                "ht4o_Getter" + propertyInfo.Name,
                typeof(object),
                new[] { typeof(object) },
                propertyInfo.Module,
                true) { InitLocals = false };

            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (propertyInfo.DeclaringType.IsValueType) {
                generator.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
            }

            generator.Emit(OpCodes.Call, propertyInfo.GetGetMethod(true));
            if (propertyInfo.PropertyType.IsValueType) {
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }

            generator.Emit(OpCodes.Ret);

            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        ///     Creates a getter function for the field info specified.
        /// </summary>
        /// <param name="fieldInfo">
        ///     The field info.
        /// </param>
        /// <returns>
        ///     The getter function created or null.
        /// </returns>
        public static Func<object, object> CreateGetter(FieldInfo fieldInfo)
        {
            if (fieldInfo == null || fieldInfo.DeclaringType == null)
            {
                return null;
            }

            var method = new DynamicMethod(
                "ht4o_Getter" + fieldInfo.Name,
                typeof(object),
                new[] { typeof(object) },
                fieldInfo.Module,
                true) { InitLocals = false };

            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (fieldInfo.DeclaringType.IsValueType) {
                generator.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
            }

            generator.Emit(OpCodes.Ldfld, fieldInfo);
            if (fieldInfo.FieldType.IsValueType) {
                generator.Emit(OpCodes.Box, fieldInfo.FieldType);
            }

            generator.Emit(OpCodes.Ret);

            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        ///     Creates an indexer function for the indexer property specified.
        /// </summary>
        /// <param name="propertyInfo">
        ///     The property info.
        /// </param>
        /// <returns>
        ///     The indexer function created or null.
        /// </returns>
        public static Func<object, int, object> CreateIndexerGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || propertyInfo.DeclaringType == null)
            {
                return null;
            }

            var method = new DynamicMethod(
                "ht4o_IndexerGetter" + propertyInfo.Name,
                typeof(object),
                new[] { typeof(object), typeof(int) },
                propertyInfo.Module,
                true) { InitLocals = false };

            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (propertyInfo.DeclaringType.IsValueType) {
                generator.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
            }

            generator.Emit(OpCodes.Ldarg_1);

            generator.Emit(OpCodes.Call, propertyInfo.GetGetMethod(true));
            if (propertyInfo.PropertyType.IsValueType) {
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }

            generator.Emit(OpCodes.Ret);

            return (Func<object, int, object>)method.CreateDelegate(typeof(Func<object, int, object>));
        }

        /// <summary>
        ///     Creates an indexer action for the indexer property specified.
        /// </summary>
        /// <param name="propertyInfo">
        ///     The property info.
        /// </param>
        /// <returns>
        ///     The indexer action created or null.
        /// </returns>
        public static Action<object, int, object> CreateIndexerSetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || propertyInfo.DeclaringType == null)
            {
                return null;
            }

            var method = new DynamicMethod(
                "ht4o_IndexerSetter" + propertyInfo.Name,
                typeof(void),
                new[] { typeof(object), typeof(int), typeof(object) },
                propertyInfo.Module,
                true) { InitLocals = false };

            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (propertyInfo.DeclaringType.IsValueType) {
                generator.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
            }

            generator.Emit(OpCodes.Ldarg_1);

            generator.Emit(OpCodes.Ldarg_2);
            if (propertyInfo.PropertyType.IsValueType) {
                generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
            }

            generator.Emit(OpCodes.Call, propertyInfo.GetSetMethod(true));
            generator.Emit(OpCodes.Ret);

            return (Action<object, int, object>)method.CreateDelegate(typeof(Action<object, int, object>));
        }

        /// <summary>
        ///     Creates a getter action for the property info specified.
        /// </summary>
        /// <param name="propertyInfo">
        ///     The property info.
        /// </param>
        /// <returns>
        ///     The getter action created or null.
        /// </returns>
        public static Action<object, object> CreateSetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || propertyInfo.DeclaringType == null)
            {
                return null;
            }

            if (propertyInfo.GetSetMethod(true) == null) {
                throw new ArgumentException($"Property {propertyInfo.Name} has no setter");
            }

            var method = new DynamicMethod(
                "ht4o_Setter" + propertyInfo.Name,
                typeof(void),
                new[] { typeof(object), typeof(object) },
                propertyInfo.Module,
                true) { InitLocals = false };

            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (propertyInfo.DeclaringType.IsValueType) {
                generator.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
            }

            generator.Emit(OpCodes.Ldarg_1);
            if (propertyInfo.PropertyType.IsValueType) {
                generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
            }

            generator.Emit(OpCodes.Call, propertyInfo.GetSetMethod(true));
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        ///     Creates a getter action for the field info specified.
        /// </summary>
        /// <param name="fieldInfo">
        ///     The field info.
        /// </param>
        /// <returns>
        ///     The getter action created or null.
        /// </returns>
        public static Action<object, object> CreateSetter(FieldInfo fieldInfo)
        {
            if (fieldInfo == null || fieldInfo.DeclaringType == null)
            {
                return null;
            }

            var method = new DynamicMethod(
                "ht4o_Setter" + fieldInfo.Name,
                typeof(void),
                new[] { typeof(object), typeof(object) },
                fieldInfo.Module,
                true) { InitLocals = false };

            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (fieldInfo.DeclaringType.IsValueType) {
                generator.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
            }

            generator.Emit(OpCodes.Ldarg_1);
            if (fieldInfo.FieldType.IsValueType) {
                generator.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
            }

            generator.Emit(OpCodes.Stfld, fieldInfo);
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Create accessors for the given expression.
        /// </summary>
        /// <param name="propertyLambda">
        ///     The property lambda expression.
        /// </param>
        /// <param name="getter">
        ///     The getter function.
        /// </param>
        /// <param name="setter">
        ///     The setter method.
        /// </param>
        /// <typeparam name="T">
        ///     The declaring type.
        /// </typeparam>
        /// <typeparam name="TProperty">
        ///     The property type.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="propertyLambda" /> is null.
        /// </exception>
        internal static void CreateAccessors<T, TProperty>(Expression<Func<T, TProperty>> propertyLambda,
            out Func<T, TProperty> getter, out Action<T, TProperty> setter)
        {
            if (propertyLambda == null)
            {
                throw new ArgumentNullException(nameof(propertyLambda));
            }

            var member = (MemberExpression) propertyLambda.Body;
            getter = Expression.Lambda<Func<T, TProperty>>(member, propertyLambda.Parameters[0]).Compile();

            var param = Expression.Parameter(typeof(TProperty), "value");
            setter = Expression
                .Lambda<Action<T, TProperty>>(Expression.Assign(member, param), propertyLambda.Parameters[0], param)
                .Compile();
        }

        /// <summary>
        ///     Creates an convert or unbox expression depending on the instance type.
        /// </summary>
        /// <param name="instance">
        ///     The instance expression.
        /// </param>
        /// <param name="instanceType">
        ///     The instance type.
        /// </param>
        /// <returns>
        ///     The created unary expression.
        /// </returns>
        private static UnaryExpression ConvertOrUnbox(Expression instance, Type instanceType)
        {
            return instanceType.IsValueType()
                ? Expression.Unbox(instance, instanceType)
                : Expression.Convert(instance, instanceType);
        }

#endregion
    }
}