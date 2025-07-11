/*******************************************************************************
* Copyright (c) 2024 Bosch Rexroth AG
* Author: Constantin Ziesche (constantin.ziesche@bosch.com)
*
* This program and the accompanying materials are made available under the
* terms of the MIT License which is available at
* https://github.com/eclipse-basyx/basyx-dotnet/blob/main/LICENSE
*
* SPDX-License-Identifier: MIT
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BaSyx.Models.AdminShell
{
    [DataContract]
    public class Reference : IReference
    {
        [JsonIgnore, IgnoreDataMember]
        public virtual KeyType RefersTo
        {
            get
            {
                if (Keys?.Count() > 0)
                    return Keys.Last().Type;
                return KeyType.Undefined;
            }
        }

        [JsonIgnore, IgnoreDataMember]
        public IKey First
        {
            get
            {
                if (Keys?.Count() > 0)
                    return Keys.FirstOrDefault();
                return null;
            }
        }

        [DataMember(EmitDefaultValue = false, IsRequired = false, Name = "keys")]
        public IEnumerable<IKey> Keys { get; set; }

		[DataMember(EmitDefaultValue = false, IsRequired = false, Name = "type")]
		public ReferenceType Type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReference ReferredSemanticId { get; set; }

        [JsonConstructor]
        public Reference(IEnumerable<IKey> keys) : this(keys.ToArray())
        { }

        public Reference(params IKey[] keys)
        {
            keys = keys ?? throw new ArgumentNullException(nameof(keys));
            Keys = keys.ToList();
        }

        public string ToStandardizedString()
        {
            string referenceString = string.Empty;
            for (int i = 0; i < Keys.Count(); i++)
            {
                referenceString += Keys.ElementAt(i).ToStandardizedString();

                if (i + 1 == Keys.Count())
                    break;
                else
                    referenceString += ", ";
            }
            return referenceString;
        }
        public static IKey CreateReferenceKey(IReferable referable)
        {
            if (referable is IIdentifiable identifiable)
                return CreateReferenceKey(identifiable);

            return new Key(Key.GetKeyElementFromType(referable.GetType()), referable.IdShort);
        }

        public static IKey CreateReferenceKey(IIdentifiable identifiable)
        {
            return new Key(Key.GetKeyElementFromType(identifiable.GetType()), identifiable.Id);
        }

        public static bool IsEqual(IReference thisReference, IReference otherReference)
        {
            if (otherReference == null)
                return false;

            // Compare ReferenceType
            if (thisReference.Type != otherReference.Type)
                return false;

            // Compare Keys count
            var thisKeys = thisReference.Keys?.ToList() ?? new List<IKey>();
            var otherKeys = otherReference.Keys?.ToList() ?? new List<IKey>();
            if (thisKeys.Count != otherKeys.Count)
                return false;

            // Compare each key (Type and Value)
            for (int i = 0; i < thisKeys.Count; i++)
            {
                var k1 = thisKeys[i];
                var k2 = otherKeys[i];
                if (k1.Type != k2.Type || k1.Value != k2.Value)
                    return false;
            }

            return true;
        }
    }


    [DataContract]
    public class Reference<T> : Reference, IReference<T> where T : IReferable
    {
        [JsonIgnore, IgnoreDataMember]
        public override KeyType RefersTo => Key.GetKeyElementFromType(typeof(T));

        [JsonConstructor]
        public Reference(IEnumerable<IKey> keys) : this(keys.ToArray())
        { }

        public Reference(params IKey[] keys) : base(keys)
        { }

        public Reference(T element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            List<IKey> keys = new List<IKey>();
            
            if (element is IIdentifiable identifiable)
            {
                keys.Add(CreateReferenceKey(identifiable));
            }
            else if (element is IReferable referable)
            {
                if (referable.Parent != null && referable.Parent is IIdentifiable parentIdentifiable)
                    keys.Add(CreateReferenceKey(parentIdentifiable));

                keys.Add(CreateReferenceKey(referable));
            }

            Type = ReferenceType.ModelReference;
			Keys = keys;
        }

        
    }


}
