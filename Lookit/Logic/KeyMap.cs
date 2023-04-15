using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Lookit.Logic
{
    internal class KeyMap
    {
        private Dictionary<Key, Action> _map = new();

        public BindableKeys Bind(params Key[] keys)
        {
            return new BindableKeys((key, action) =>
            {
                _map[key] = action;
            }, keys.ToList());
        }

        public Action GetBinding(Key key) => _map.ContainsKey(key) ? _map[key] : null;
    }

    internal class BindableKeys
    {
        private readonly Action<Key, Action> _onBind;
        private readonly List<Key> _keys;

        public BindableKeys(Action<Key, Action> onBind, List<Key> keys)
        {
            _onBind = onBind;
            _keys = keys;
        }

        public void To(Action action)
        {
            foreach (var key in _keys)
            {
                _onBind(key, action);
            }
        }
    }
}
