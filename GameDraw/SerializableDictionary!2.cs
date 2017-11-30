namespace GameDraw
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable]
    public class SerializableDictionary<TKey, TVal> : Dictionary<TKey, TVal>, IXmlSerializable, ISerializable, ICloneable
    {
        private const string DictionaryNodeName = "Dictionary";
        private const string ItemNodeName = "Item";
        private const string KeyNodeName = "Key";
        private XmlSerializer keySerializer;
        private const string ValueNodeName = "Value";
        private XmlSerializer valueSerializer;

        public SerializableDictionary()
        {
        }

        public SerializableDictionary(IDictionary<TKey, TVal> dictionary) : base(dictionary)
        {
        }

        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer)
        {
        }

        public SerializableDictionary(int capacity) : base(capacity)
        {
        }

        public SerializableDictionary(IDictionary<TKey, TVal> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer)
        {
        }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer)
        {
        }

        protected SerializableDictionary(SerializationInfo info, StreamingContext context)
        {
            int num = info.GetInt32("ItemCount");
            for (int i = 0; i < num; i++)
            {
                KeyValuePair<TKey, TVal> pair = (KeyValuePair<TKey, TVal>) info.GetValue(string.Format("Item{0}", i), typeof(KeyValuePair<TKey, TVal>));
                base.Add(pair.Key, pair.Value);
            }
        }

        public object Clone()
        {
            SerializableDictionary<TKey, TVal> dictionary = new SerializableDictionary<TKey, TVal>();
            foreach (KeyValuePair<TKey, TVal> pair in this)
            {
                ICloneable cloneable = pair.Value as ICloneable;
                dictionary.Add(pair.Key, (TVal) cloneable.Clone());
            }
            return dictionary;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ItemCount", base.Count);
            int num = 0;
            foreach (KeyValuePair<TKey, TVal> pair in this)
            {
                info.AddValue(string.Format("Item{0}", num), pair, typeof(KeyValuePair<TKey, TVal>));
                num++;
            }
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                if (!reader.Read())
                {
                    throw new XmlException("Error in Deserialization of Dictionary");
                }
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    reader.ReadStartElement("Item");
                    reader.ReadStartElement("Key");
                    TKey key = (TKey) this.KeySerializer.Deserialize(reader);
                    reader.ReadEndElement();
                    reader.ReadStartElement("Value");
                    TVal local2 = (TVal) this.ValueSerializer.Deserialize(reader);
                    reader.ReadEndElement();
                    reader.ReadEndElement();
                    base.Add(key, local2);
                    reader.MoveToContent();
                }
                reader.ReadEndElement();
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            foreach (KeyValuePair<TKey, TVal> pair in this)
            {
                writer.WriteStartElement("Item");
                writer.WriteStartElement("Key");
                this.KeySerializer.Serialize(writer, pair.Key);
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                this.ValueSerializer.Serialize(writer, pair.Value);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        private XmlSerializer KeySerializer
        {
            get
            {
                if (this.keySerializer == null)
                {
                    this.keySerializer = new XmlSerializer(typeof(TKey));
                }
                return this.keySerializer;
            }
        }

        protected XmlSerializer ValueSerializer
        {
            get
            {
                if (this.valueSerializer == null)
                {
                    this.valueSerializer = new XmlSerializer(typeof(TVal));
                }
                return this.valueSerializer;
            }
        }
    }
}

