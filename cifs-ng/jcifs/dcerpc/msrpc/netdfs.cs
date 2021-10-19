using DcerpcMessage = jcifs.dcerpc.DcerpcMessage;
using NdrBuffer = jcifs.dcerpc.ndr.NdrBuffer;
using NdrException = jcifs.dcerpc.ndr.NdrException;
using NdrLong = jcifs.dcerpc.ndr.NdrLong;
using NdrObject = jcifs.dcerpc.ndr.NdrObject;

namespace jcifs.dcerpc.msrpc {



	public class netdfs {

		public static string getSyntax() {
			return "4fc742e0-4a10-11cf-8273-00aa004ae673:3.0";
		}

		public const int DFS_VOLUME_FLAVOR_STANDALONE = 0x100;
		public const int DFS_VOLUME_FLAVOR_AD_BLOB = 0x200;
		public const int DFS_STORAGE_STATE_OFFLINE = 0x0001;
		public const int DFS_STORAGE_STATE_ONLINE = 0x0002;
		public const int DFS_STORAGE_STATE_ACTIVE = 0x0004;

		public class DfsInfo1 : NdrObject {

			public string entry_path;


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode(NdrBuffer _dst) {
				_dst.align(4);
				_dst.enc_ndr_referent(this.entry_path, 1);

				if ((this.entry_path!=null)) {
					_dst = _dst.deferred;
					_dst.enc_ndr_string(this.entry_path);

				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode(NdrBuffer _src) {
				_src.align(4);
				int _entry_pathp = _src.dec_ndr_long();

				if (_entry_pathp != 0) {
					_src = _src.deferred;
					this.entry_path = _src.dec_ndr_string();

				}
			}
		}

		public class DfsEnumArray1 : NdrObject {

			public int count;
			public DfsInfo1[] s;


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode(NdrBuffer _dst) {
				_dst.align(4);
				_dst.enc_ndr_long(this.count);
				_dst.enc_ndr_referent(this.s, 1);

				if (this.s != null) {
					_dst = _dst.deferred;
					int _ss = this.count;
					_dst.enc_ndr_long(_ss);
					int _si = _dst.index;
					_dst.advance(4 * _ss);

					_dst = _dst.derive(_si);
					for (int _i = 0; _i < _ss; _i++) {
						this.s[_i].encode(_dst);
					}
				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode(NdrBuffer _src) {
				_src.align(4);
				this.count = _src.dec_ndr_long();
				int _sp = _src.dec_ndr_long();

				if (_sp != 0) {
					_src = _src.deferred;
					int _ss = _src.dec_ndr_long();
					int _si = _src.index;
					_src.advance(4 * _ss);

					if (this.s == null) {
						if (_ss < 0 || _ss > 0xFFFF) {
							throw new NdrException(NdrException.INVALID_CONFORMANCE);
						}
						this.s = new DfsInfo1[_ss];
					}
					_src = _src.derive(_si);
					for (int _i = 0; _i < _ss; _i++) {
						if (this.s[_i] == null) {
							this.s[_i] = new DfsInfo1();
						}
						this.s[_i].decode(_src);
					}
				}
			}
		}

		public class DfsStorageInfo : NdrObject {

			public int state;
			public string server_name;
			public string share_name;


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode(NdrBuffer _dst) {
				_dst.align(4);
				_dst.enc_ndr_long(this.state);
				_dst.enc_ndr_referent(this.server_name, 1);
				_dst.enc_ndr_referent(this.share_name, 1);

				if ((this.server_name!= null)) {
					_dst = _dst.deferred;
					_dst.enc_ndr_string(this.server_name);

				}
				if ((this.share_name!= null)) {
					_dst = _dst.deferred;
					_dst.enc_ndr_string(this.share_name);

				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode(NdrBuffer _src) {
				_src.align(4);
				this.state = _src.dec_ndr_long();
				int _server_namep = _src.dec_ndr_long();
				int _share_namep = _src.dec_ndr_long();

				if (_server_namep != 0) {
					_src = _src.deferred;
					this.server_name = _src.dec_ndr_string();

				}
				if (_share_namep != 0) {
					_src = _src.deferred;
					this.share_name = _src.dec_ndr_string();

				}
			}
		}

		public class DfsInfo3 : NdrObject {

			public string path;
			public string comment;
			public int state;
			public int num_stores;
			public DfsStorageInfo[] stores;


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode(NdrBuffer _dst) {
				_dst.align(4);
				_dst.enc_ndr_referent(this.path, 1);
				_dst.enc_ndr_referent(this.comment, 1);
				_dst.enc_ndr_long(this.state);
				_dst.enc_ndr_long(this.num_stores);
				_dst.enc_ndr_referent(this.stores, 1);

				if ((this.path!= null)) {
					_dst = _dst.deferred;
					_dst.enc_ndr_string(this.path);

				}
				if ((this.comment!= null)) {
					_dst = _dst.deferred;
					_dst.enc_ndr_string(this.comment);

				}
				if (this.stores != null) {
					_dst = _dst.deferred;
					int _storess = this.num_stores;
					_dst.enc_ndr_long(_storess);
					int _storesi = _dst.index;
					_dst.advance(12 * _storess);

					_dst = _dst.derive(_storesi);
					for (int _i = 0; _i < _storess; _i++) {
						this.stores[_i].encode(_dst);
					}
				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode(NdrBuffer _src) {
				_src.align(4);
				int _pathp = _src.dec_ndr_long();
				int _commentp = _src.dec_ndr_long();
				this.state = _src.dec_ndr_long();
				this.num_stores = _src.dec_ndr_long();
				int _storesp = _src.dec_ndr_long();

				if (_pathp != 0) {
					_src = _src.deferred;
					this.path = _src.dec_ndr_string();

				}
				if (_commentp != 0) {
					_src = _src.deferred;
					this.comment = _src.dec_ndr_string();

				}
				if (_storesp != 0) {
					_src = _src.deferred;
					int _storess = _src.dec_ndr_long();
					int _storesi = _src.index;
					_src.advance(12 * _storess);

					if (this.stores == null) {
						if (_storess < 0 || _storess > 0xFFFF) {
							throw new NdrException(NdrException.INVALID_CONFORMANCE);
						}
						this.stores = new DfsStorageInfo[_storess];
					}
					_src = _src.derive(_storesi);
					for (int _i = 0; _i < _storess; _i++) {
						if (this.stores[_i] == null) {
							this.stores[_i] = new DfsStorageInfo();
						}
						this.stores[_i].decode(_src);
					}
				}
			}
		}

		public class DfsEnumArray3 : NdrObject {

			public int count;
			public DfsInfo3[] s;


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode(NdrBuffer _dst) {
				_dst.align(4);
				_dst.enc_ndr_long(this.count);
				_dst.enc_ndr_referent(this.s, 1);

				if (this.s != null) {
					_dst = _dst.deferred;
					int _ss = this.count;
					_dst.enc_ndr_long(_ss);
					int _si = _dst.index;
					_dst.advance(20 * _ss);

					_dst = _dst.derive(_si);
					for (int _i = 0; _i < _ss; _i++) {
						this.s[_i].encode(_dst);
					}
				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode(NdrBuffer _src) {
				_src.align(4);
				this.count = _src.dec_ndr_long();
				int _sp = _src.dec_ndr_long();

				if (_sp != 0) {
					_src = _src.deferred;
					int _ss = _src.dec_ndr_long();
					int _si = _src.index;
					_src.advance(20 * _ss);

					if (this.s == null) {
						if (_ss < 0 || _ss > 0xFFFF) {
							throw new NdrException(NdrException.INVALID_CONFORMANCE);
						}
						this.s = new DfsInfo3[_ss];
					}
					_src = _src.derive(_si);
					for (int _i = 0; _i < _ss; _i++) {
						if (this.s[_i] == null) {
							this.s[_i] = new DfsInfo3();
						}
						this.s[_i].decode(_src);
					}
				}
			}
		}

		public class DfsInfo200 : NdrObject {

			public string dfs_name;


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode(NdrBuffer _dst) {
				_dst.align(4);
				_dst.enc_ndr_referent(this.dfs_name, 1);

				if ((this.dfs_name!= null)) {
					_dst = _dst.deferred;
					_dst.enc_ndr_string(this.dfs_name);

				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode(NdrBuffer _src) {
				_src.align(4);
				int _dfs_namep = _src.dec_ndr_long();

				if (_dfs_namep != 0) {
					_src = _src.deferred;
					this.dfs_name = _src.dec_ndr_string();

				}
			}
		}

		public class DfsEnumArray200 : NdrObject {

			public int count;
			public DfsInfo200[] s;


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode(NdrBuffer _dst) {
				_dst.align(4);
				_dst.enc_ndr_long(this.count);
				_dst.enc_ndr_referent(this.s, 1);

				if (this.s != null) {
					_dst = _dst.deferred;
					int _ss = this.count;
					_dst.enc_ndr_long(_ss);
					int _si = _dst.index;
					_dst.advance(4 * _ss);

					_dst = _dst.derive(_si);
					for (int _i = 0; _i < _ss; _i++) {
						this.s[_i].encode(_dst);
					}
				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode(NdrBuffer _src) {
				_src.align(4);
				this.count = _src.dec_ndr_long();
				int _sp = _src.dec_ndr_long();

				if (_sp != 0) {
					_src = _src.deferred;
					int _ss = _src.dec_ndr_long();
					int _si = _src.index;
					_src.advance(4 * _ss);

					if (this.s == null) {
						if (_ss < 0 || _ss > 0xFFFF) {
							throw new NdrException(NdrException.INVALID_CONFORMANCE);
						}
						this.s = new DfsInfo200[_ss];
					}
					_src = _src.derive(_si);
					for (int _i = 0; _i < _ss; _i++) {
						if (this.s[_i] == null) {
							this.s[_i] = new DfsInfo200();
						}
						this.s[_i].decode(_src);
					}
				}
			}
		}

		public class DfsInfo300 : NdrObject {

			public int flags;
			public string dfs_name;


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode(NdrBuffer _dst) {
				_dst.align(4);
				_dst.enc_ndr_long(this.flags);
				_dst.enc_ndr_referent(this.dfs_name, 1);

				if ((this.dfs_name!= null)) {
					_dst = _dst.deferred;
					_dst.enc_ndr_string(this.dfs_name);

				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode(NdrBuffer _src) {
				_src.align(4);
				this.flags = _src.dec_ndr_long();
				int _dfs_namep = _src.dec_ndr_long();

				if (_dfs_namep != 0) {
					_src = _src.deferred;
					this.dfs_name = _src.dec_ndr_string();

				}
			}
		}

		public class DfsEnumArray300 : NdrObject {

			public int count;
			public DfsInfo300[] s;


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode(NdrBuffer _dst) {
				_dst.align(4);
				_dst.enc_ndr_long(this.count);
				_dst.enc_ndr_referent(this.s, 1);

				if (this.s != null) {
					_dst = _dst.deferred;
					int _ss = this.count;
					_dst.enc_ndr_long(_ss);
					int _si = _dst.index;
					_dst.advance(8 * _ss);

					_dst = _dst.derive(_si);
					for (int _i = 0; _i < _ss; _i++) {
						this.s[_i].encode(_dst);
					}
				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode(NdrBuffer _src) {
				_src.align(4);
				this.count = _src.dec_ndr_long();
				int _sp = _src.dec_ndr_long();

				if (_sp != 0) {
					_src = _src.deferred;
					int _ss = _src.dec_ndr_long();
					int _si = _src.index;
					_src.advance(8 * _ss);

					if (this.s == null) {
						if (_ss < 0 || _ss > 0xFFFF) {
							throw new NdrException(NdrException.INVALID_CONFORMANCE);
						}
						this.s = new DfsInfo300[_ss];
					}
					_src = _src.derive(_si);
					for (int _i = 0; _i < _ss; _i++) {
						if (this.s[_i] == null) {
							this.s[_i] = new DfsInfo300();
						}
						this.s[_i].decode(_src);
					}
				}
			}
		}

		public class DfsEnumStruct : NdrObject {

			public int level;
			public NdrObject e;


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode(NdrBuffer _dst) {
				_dst.align(4);
				_dst.enc_ndr_long(this.level);
				int _descr = this.level;
				_dst.enc_ndr_long(_descr);
				_dst.enc_ndr_referent(this.e, 1);

				if (this.e != null) {
					_dst = _dst.deferred;
					this.e.encode(_dst);

				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode(NdrBuffer _src) {
				_src.align(4);
				this.level = _src.dec_ndr_long();
				_src.dec_ndr_long(); // union discriminant
				int _ep = _src.dec_ndr_long();

				if (_ep != 0) {
					if (this.e == null) {
						this.e = new DfsEnumArray1();
					}
					_src = _src.deferred;
					this.e.decode(_src);

				}
			}
		}

		public class NetrDfsEnumEx : DcerpcMessage {

			public override int getOpnum() {
				return 0x15;
			}

			public int retval;
			public string dfs_name;
			public int level;
			public int prefmaxlen;
			public DfsEnumStruct info;
			public NdrLong totalentries;


			public NetrDfsEnumEx(string dfs_name, int level, int prefmaxlen, DfsEnumStruct info, NdrLong totalentries) {
				this.dfs_name = dfs_name;
				this.level = level;
				this.prefmaxlen = prefmaxlen;
				this.info = info;
				this.totalentries = totalentries;
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void encode_in(NdrBuffer _dst) {
				_dst.enc_ndr_string(this.dfs_name);
				_dst.enc_ndr_long(this.level);
				_dst.enc_ndr_long(this.prefmaxlen);
				_dst.enc_ndr_referent(this.info, 1);
				if (this.info != null) {
					this.info.encode(_dst);

				}
				_dst.enc_ndr_referent(this.totalentries, 1);
				if (this.totalentries != null) {
					this.totalentries.encode(_dst);

				}
			}


		/// throws jcifs.dcerpc.ndr.NdrException
			public override void decode_out(NdrBuffer _src) {
				int _infop = _src.dec_ndr_long();
				if (_infop != 0) {
					if (this.info == null) {
						this.info = new DfsEnumStruct();
					}
					this.info.decode(_src);

				}
				int _totalentriesp = _src.dec_ndr_long();
				if (_totalentriesp != 0) {
					this.totalentries.decode(_src);

				}
				this.retval = _src.dec_ndr_long();
			}
		}
	}

}