/** -*- C# -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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

    /// <summary>
    /// The delegate factory.
    /// </summary>
    internal static class DelegateFactory
    {
        #region Methods

        /// <summary>
        /// Create accessors for the given expression.
        /// </summary>
        /// <param name="propertyLambda">
        /// The property lambda expression.
        /// </param>
        /// <param name="getter">
        /// The getter function.
        /// </param>
        /// <param name="setter">
        /// The setter method.
        /// </param>
        /// <typeparam name="T">
        /// The declaring type.
        /// </typeparam>
        /// <typeparam name="TProperty">
        /// The property type.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="propertyLambda"/> is null.
        /// </exception>
        internal static void CreateAccessors<T, TProperty>(Expression<Func<T, TProperty>> propertyLambda, out Func<T, TProperty> getter, out Action<T, TProperty> setter)
        {
            if (propertyLambda == null)
            {
                throw new ArgumentNullException("propertyLambda");
            }

            var member = (MemberExpression)propertyLambda.Body;
            getter = Expression.Lambda<Func<T, TProperty>>(member, propertyLambda.Parameters[0]).Compile();

            var param = Expression.Parameter(typeof(TProperty), "value");
            setter = Expression.Lambda<Action<T, TProperty>>(Expression.Assign(member, param), propertyLambda.Parameters[0], param).Compile();
        }

        /// <summary>
        /// Create an action for the method info specified.
        /// </summary>
        /// <param name="methodInfo">
        /// The method info.
        /// </param>
        /// <returns>
        /// The action created or null.
        /// </returns>
        internal static Action<object, object> CreateAction(MethodInfo methodInfo)
        {
            if (methodInfo == null || methodInfo.GetParameters().Length != 1 || methodInfo.ReturnType != typeof(void))
            {
                return null;
            }

            var genericHelper = typeof(DelegateFactory).GetMethod("CreateDelegateAction", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(methodInfo.DeclaringType, methodInfo.GetParameters()[0].ParameterType);
            return (Action<object, object>)constructedHelper.Invoke(null, new object[] { methodInfo });
        }

        /// <summary>
        /// Creates a function for the method info specified.
        /// </summary>
        /// <param name="methodInfo">
        /// The method info.
        /// </param>
        /// <returns>
        /// The function created or null.
        /// </returns>
        internal static Func<object, object, object> CreateFunc(MethodInfo methodInfo)
        {
            if (methodInfo == null || methodInfo.GetParameters().Length != 1 || methodInfo.ReturnType == typeof(void))
            {
                return null;
            }

            var genericHelper = typeof(DelegateFactory).GetMethod("CreateDelegateFunc", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(methodInfo.DeclaringType, methodInfo.GetParameters()[0].ParameterType, methodInfo.ReturnType);
            return (Func<object, object, object>)constructedHelper.Invoke(null, new object[] { methodInfo });
        }

        /// <summary>
        /// Creates a getter function for the property info specified.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <returns>
        /// The getter function created or null.
        /// </returns>
        internal static Func<object, object> CreateGetterFunc(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || propertyInfo.DeclaringType == null)
            {
                return null;
            }

            if (!propertyInfo.DeclaringType.IsValueType)
            {
                var genericHelper = typeof(DelegateFactory).GetMethod("CreateDelegateGetterFunc", BindingFlags.Static | BindingFlags.NonPublic);
                var constructedHelper = genericHelper.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
                return (Func<object, object>)constructedHelper.Invoke(null, new object[] { propertyInfo });
            }

            var method = new DynamicMethod("Getter" + propertyInfo.Name, typeof(object), new[] { typeof(object) }, propertyInfo.Module, true);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (propertyInfo.DeclaringType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
            }

            generator.EmitCall(OpCodes.Callvirt, propertyInfo.GetGetMethod(true), null);
            if (propertyInfo.PropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Box, propertyInfo.PropertyType);
            }

            generator.Emit(OpCodes.Ret);

            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// Creates a getter function for the field info specified.
        /// </summary>
        /// <param name="fieldInfo">
        /// The field info.
        /// </param>
        /// <returns>
        /// The getter function created or null.
        /// </returns>
        internal static Func<object, object> CreateGetterFunc(FieldInfo fieldInfo)
        {
            if (fieldInfo == null || fieldInfo.DeclaringType == null)
            {
                return null;
            }

            var method = new DynamicMethod("Getter" + fieldInfo.Name, typeof(object), new[] { typeof(object) }, fieldInfo.Module, true);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (fieldInfo.DeclaringType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
            }

            generator.Emit(OpCodes.Ldfld, fieldInfo);
            if (fieldInfo.FieldType.IsValueType)
            {
                generator.Emit(OpCodes.Box, fieldInfo.FieldType);
            }

            generator.Emit(OpCodes.Ret);

            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// Creates an indexer action for the indexer property specified.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <returns>
        /// The indexer action created or null.
        /// </returns>
        internal static Action<object, int, object> CreateIndexerAction(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return null;
            }

            var genericHelper = typeof(DelegateFactory).GetMethod("CreateDelegateIndexerAction", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (Action<object, int, object>)constructedHelper.Invoke(null, new object[] { propertyInfo });
        }

        /// <summary>
        /// Creates an indexer function for the indexer property specified.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <returns>
        /// The indexer function created or null.
        /// </returns>
        internal static Func<object, int, object> CreateIndexerFunc(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return null;
            }

            var genericHelper = typeof(DelegateFactory).GetMethod("CreateDelegatIndexerFunc", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (Func<object, int, object>)constructedHelper.Invoke(null, new object[] { propertyInfo });
        }

        /// <summary>
        /// Creates a getter action for the property info specified.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <returns>
        /// The getter action created or null.
        /// </returns>
        internal static Action<object, object> CreateSetterAction(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || propertyInfo.DeclaringType == null)
            {
                return null;
            }

            if (!propertyInfo.DeclaringType.IsValueType)
            {
                var genericHelper = typeof(DelegateFactory).GetMethod("CreateDelegateSetterAction", BindingFlags.Static | BindingFlags.NonPublic);
                var constructedHelper = genericHelper.MakeGenericMethod(propertyInfo.DeclaringType, propertyInfo.PropertyType);
                return (Action<object, object>)constructedHelper.Invoke(null, new object[] { propertyInfo });
            }

            var method = new DynamicMethod("Setter" + propertyInfo.Name, typeof(void), new[] { typeof(object), typeof(object) }, propertyInfo.Module, true);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Unbox, propertyInfo.DeclaringType);
            generator.Emit(OpCodes.Ldarg_1);
            if (propertyInfo.PropertyType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
            }

            generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod(true));
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        /// Creates a getter action for the field info specified.
        /// </summary>
        /// <param name="fieldInfo">
        /// The field info.
        /// </param>
        /// <returns>
        /// The getter action created or null.
        /// </returns>
        internal static Action<object, object> CreateSetterAction(FieldInfo fieldInfo)
        {
            if (fieldInfo == null || fieldInfo.DeclaringType == null)
            {
                return null;
            }

            var method = new DynamicMethod("Setter" + fieldInfo.Name, typeof(void), new[] { typeof(object), typeof(object) }, fieldInfo.Module, true);
            var generator = method.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            if (fieldInfo.DeclaringType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
            }

            generator.Emit(OpCodes.Ldarg_1);
            if (fieldInfo.FieldType.IsValueType)
            {
                generator.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
            }

            generator.Emit(OpCodes.Stfld, fieldInfo);
            generator.Emit(OpCodes.Ret);

            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        /// Creates a delegate action for the method info specified.
        /// </summary>
        /// <param name="methodInfo">
        /// The method info.
        /// </param>
        /// <typeparam name="TTarget">
        /// The target type.
        /// </typeparam>
        /// <typeparam name="TParam">
        /// The parameter type.
        /// </typeparam>
        /// <returns>
        /// The created action.
        /// </returns>
        private static Action<object, object> CreateDelegateAction<TTarget, TParam>(MethodInfo methodInfo) where TTarget : class
        {
            if (methodInfo == null)
            {
                return null;
            }

            var func = (Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), methodInfo);
            return (target, param) => func((TTarget)target, (TParam)param);
        }

        /// <summary>
        /// Creates a delegate function for the method info specified.
        /// </summary>
        /// <param name="methodInfo">
        /// The method info.
        /// </param>
        /// <typeparam name="TTarget">
        /// The target type.
        /// </typeparam>
        /// <typeparam name="TParam">
        /// The parameter type.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// The result type.
        /// </typeparam>
        /// <returns>
        /// The created function.
        /// </returns>
        private static Func<object, object, object> CreateDelegateFunc<TTarget, TParam, TResult>(MethodInfo methodInfo) where TTarget : class
        {
            if (methodInfo == null)
            {
                return null;
            }

            var func = (Func<TTarget, TParam, TResult>)Delegate.CreateDelegate(typeof(Func<TTarget, TParam, TResult>), methodInfo);
            return (target, param) => func((TTarget)target, (TParam)param);
        }

        /// <summary>
        /// Creates a delegate getter function for the property info specified.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <typeparam name="TTarget">
        /// The target type.
        /// </typeparam>
        /// <typeparam name="TParam">
        /// The parameter type.
        /// </typeparam>
        /// <returns>
        /// The created getter function.
        /// </returns>
        private static Func<object, object> CreateDelegateGetterFunc<TTarget, TParam>(PropertyInfo propertyInfo) where TTarget : class
        {
            var m = propertyInfo.GetGetMethod(true);
            if (m == null)
            {
                return null;
            }

            var func = (Func<TTarget, TParam>)Delegate.CreateDelegate(typeof(Func<TTarget, TParam>), m);
            return target => func((TTarget)target);
        }

        /// <summary>
        /// Creates a delegate indexer action for the indexer property info specified.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <typeparam name="TTarget">
        /// The target type.
        /// </typeparam>
        /// <typeparam name="TParam">
        /// The parameter type.
        /// </typeparam>
        /// <returns>
        /// The created indexer action.
        /// </returns>
        private static Action<object, int, object> CreateDelegateIndexerAction<TTarget, TParam>(PropertyInfo propertyInfo) where TTarget : class
        {
            var m = propertyInfo.GetSetMethod(true);
            if (m == null)
            {
                return null;
            }

            var func = (Action<TTarget, int, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, int, TParam>), m);
            return (target, index, param) => func((TTarget)target, index, (TParam)param);
        }

        /// <summary>
        /// Creates a delegate indexer function for the indexer property info specified.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <typeparam name="TTarget">
        /// The target type.
        /// </typeparam>
        /// <typeparam name="TParam">
        /// The parameter type.
        /// </typeparam>
        /// <returns>
        /// The created indexer function.
        /// </returns>
        private static Func<object, int, object> CreateDelegateIndexerFunc<TTarget, TParam>(PropertyInfo propertyInfo) where TTarget : class
        {
            var m = propertyInfo.GetGetMethod(true);
            if (m == null)
            {
                return null;
            }

            var func = (Func<TTarget, int, TParam>)Delegate.CreateDelegate(typeof(Func<TTarget, int, TParam>), m);
            return (target, index) => func((TTarget)target, index);
        }

        /// <summary>
        /// Creates a delegate setter action for the property info specified.
        /// </summary>
        /// <param name="propertyInfo">
        /// The property info.
        /// </param>
        /// <typeparam name="TTarget">
        /// The target type.
        /// </typeparam>
        /// <typeparam name="TParam">
        /// The parameter type.
        /// </typeparam>
        /// <returns>
        /// The created setter action.
        /// </returns>
        private static Action<object, object> CreateDelegateSetterAction<TTarget, TParam>(PropertyInfo propertyInfo) where TTarget : class
        {
            var m = propertyInfo.GetSetMethod(true);
            if (m == null)
            {
                return null;
            }

            var func = (Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), m);
            return (target, param) => func((TTarget)target, (TParam)param);
        }

        #endregion
    }
}