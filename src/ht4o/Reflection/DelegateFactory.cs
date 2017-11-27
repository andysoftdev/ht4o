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

            var instanceType = methodInfo.DeclaringType;
            var argumentType = methodInfo.GetParameters()[0].ParameterType;

            var instance = Expression.Parameter(typeof(object));
            var argument = Expression.Parameter(typeof(object));
            var createExpression =
                Expression.Lambda<Action<object, object>>(
                    Expression.Call(ConvertOrUnbox(instance, instanceType), methodInfo,
                        ConvertOrUnbox(argument, argumentType)), instance, argument);

            return createExpression.Compile();
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

            var instanceType = methodInfo.DeclaringType;
            var argumentType = methodInfo.GetParameters()[0].ParameterType;

            var instance = Expression.Parameter(typeof(object));
            var argument = Expression.Parameter(typeof(object));
            var createExpression =
                Expression.Lambda<Func<object, object, object>>(
                    Expression.Convert(
                        Expression.Call(ConvertOrUnbox(instance, instanceType), methodInfo,
                            ConvertOrUnbox(argument, argumentType)), typeof(object)),
                    instance,
                    argument);

            return createExpression.Compile();
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

            var instanceType = propertyInfo.DeclaringType;

            var instance = Expression.Parameter(typeof(object));
            var createExpression =
                Expression.Lambda<Func<object, object>>(
                    Expression.Convert(Expression.Property(ConvertOrUnbox(instance, instanceType), propertyInfo),
                        typeof(object)), instance);

            return createExpression.Compile();
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

            var instanceType = fieldInfo.DeclaringType;

            var instance = Expression.Parameter(typeof(object));
            var createExpression = Expression.Lambda<Func<object, object>>(
                Expression.Convert(Expression.Field(ConvertOrUnbox(instance, instanceType), fieldInfo), typeof(object)),
                instance);

            return createExpression.Compile();
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

            var instanceType = propertyInfo.DeclaringType;

            var instance = Expression.Parameter(typeof(object));
            var index = Expression.Parameter(typeof(int));
            var createExpression =
                Expression.Lambda<Func<object, int, object>>(
                    Expression.Convert(
                        Expression.MakeIndex(ConvertOrUnbox(instance, instanceType), propertyInfo, new[] {index}),
                        typeof(object)), instance, index);

            return createExpression.Compile();
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

            var instanceType = propertyInfo.DeclaringType;
            var elementType = propertyInfo.PropertyType;

            var instance = Expression.Parameter(typeof(object));
            var index = Expression.Parameter(typeof(int));
            var newValue = Expression.Parameter(typeof(object));
            var createExpression =
                Expression.Lambda<Action<object, int, object>>(
                    Expression.Assign(
                        Expression.MakeIndex(ConvertOrUnbox(instance, instanceType), propertyInfo, new[] {index}),
                        ConvertOrUnbox(newValue, elementType)),
                    instance,
                    index,
                    newValue);

            return createExpression.Compile();
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

            var instanceType = propertyInfo.DeclaringType;
            var propertyType = propertyInfo.PropertyType;

            var instance = Expression.Parameter(typeof(object));
            var newValue = Expression.Parameter(typeof(object));
            var createExpression =
                Expression.Lambda<Action<object, object>>(
                    Expression.Assign(Expression.Property(ConvertOrUnbox(instance, instanceType), propertyInfo),
                        ConvertOrUnbox(newValue, propertyType)), instance, newValue);

            return createExpression.Compile();
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

            if (fieldInfo.IsInitOnly)
            {
                var method = new DynamicMethod("Setter" + fieldInfo.Name, typeof(void),
                    new[] {typeof(object), typeof(object)}, fieldInfo.Module, true);

                var generator = method.GetILGenerator();

                generator.Emit(OpCodes.Ldarg_0);
                if (fieldInfo.DeclaringType.IsValueType())
                {
                    generator.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
                }

                generator.Emit(OpCodes.Ldarg_1);
                if (fieldInfo.FieldType.IsValueType())
                {
                    generator.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
                }

                generator.Emit(OpCodes.Stfld, fieldInfo);
                generator.Emit(OpCodes.Ret);

                return (Action<object, object>) method.CreateDelegate(typeof(Action<object, object>));
            }

            var instanceType = fieldInfo.DeclaringType;
            var propertyType = fieldInfo.FieldType;

            var instance = Expression.Parameter(typeof(object));
            var newValue = Expression.Parameter(typeof(object));
            var createExpression =
                Expression.Lambda<Action<object, object>>(
                    Expression.Assign(Expression.Field(ConvertOrUnbox(instance, instanceType), fieldInfo),
                        ConvertOrUnbox(newValue, propertyType)), instance, newValue);

            return createExpression.Compile();
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