using System;
using System.IO;
using Org.BouncyCastle.Asn1; /* jcifs smb client library in Java
 * Copyright (C) 2004  "Michael B. Allen" <jcifs at samba dot org>
 *                   "Eric Glass" <jcifs at samba dot org>
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

namespace jcifs.spnego
{
    public class NegTokenTarg : SpnegoToken
    {
        public const int UNSPECIFIED_RESULT = -1;
        public const int ACCEPT_COMPLETED = 0;
        public const int ACCEPT_INCOMPLETE = 1;
        public const int REJECTED = 2;
        public const int REQUEST_MIC = 3;

        private DerObjectIdentifier mechanism;

        private int result = UNSPECIFIED_RESULT;


        public NegTokenTarg()
        {
        }


        public NegTokenTarg(int result, DerObjectIdentifier mechanism, byte[] mechanismToken, byte[] mechanismListMIC)
        {
            setResult(result);
            setMechanism(mechanism);
            setMechanismToken(mechanismToken);
            setMechanismListMIC(mechanismListMIC);
        }


		/// throws java.io.IOException
        public NegTokenTarg(byte[] token)
        {
            parse(token);
        }


        public virtual int getResult()
        {
            return this.result;
        }


        public virtual void setResult(int result)
        {
            this.result = result;
        }


        public virtual DerObjectIdentifier getMechanism()
        {
            return this.mechanism;
        }


        public virtual void setMechanism(DerObjectIdentifier mechanism)
        {
            this.mechanism = mechanism;
        }


        public override byte[] toByteArray()
        {
            try
            {
                MemoryStream collector = new MemoryStream();
                Asn1OutputStream der = new Asn1OutputStream(collector);
                Asn1EncodableVector fields = new Asn1EncodableVector();
                int res = getResult();
                if (res != UNSPECIFIED_RESULT)
                {
                    fields.Add(new DerTaggedObject(true, 0, new DerEnumerated(res)));
                }

                DerObjectIdentifier mech = getMechanism();
                if (mech != null)
                {
                    fields.Add(new DerTaggedObject(true, 1, mech));
                }

                byte[] mechanismToken = getMechanismToken();
                if (mechanismToken != null)
                {
                    fields.Add(new DerTaggedObject(true, 2, new DerOctetString(mechanismToken)));
                }

                byte[] mechanismListMIC = getMechanismListMIC();
                if (mechanismListMIC != null)
                {
                    fields.Add(new DerTaggedObject(true, 3, new DerOctetString(mechanismListMIC)));
                }

                der.WriteObject(new DerTaggedObject(true, 1, new DerSequence(fields)));
                return collector.ToArray();
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }


		/// throws java.io.IOException
        protected internal override void parse(byte[] token)
        {
            using (Asn1InputStream der = new Asn1InputStream(token))
            {
                Asn1TaggedObject tagged = (Asn1TaggedObject) der.ReadObject();
                Asn1Sequence sequence = Asn1Sequence.GetInstance(tagged, true);
		//TODO type  java.util.Iterator<?> fields = sequence.getObjects();
                var fields = sequence.GetEnumerator();
                while (fields.MoveNext())
                {
                    tagged = (Asn1TaggedObject) fields.Current;
                    switch (tagged.TagNo)
                    {
                        case 0:
                            var enumerated = DerEnumerated.GetInstance(tagged, true);
                            setResult(enumerated.Value.IntValue);
                            break;
                        case 1:
                            setMechanism(DerObjectIdentifier.GetInstance(tagged, true));
                            break;
                        case 2:
                            Asn1OctetString mechanismToken = Asn1OctetString.GetInstance(tagged, true);
                            setMechanismToken(mechanismToken.GetOctets());
                            break;
                        case 3:
                            Asn1OctetString mechanismListMIC = Asn1OctetString.GetInstance(tagged, true);
                            setMechanismListMIC(mechanismListMIC.GetOctets());
                            break;
                        default:
                            throw new IOException("Malformed token field.");
                    }
                }
            }
        }
    }
}