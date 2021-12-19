using Cheap2.Plugin.Pool;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModuleBased.Example.Drama
{
    [RequireComponent(typeof(MaskableGraphic))]
    public class UIAgent : MonoBehaviour, IDisposable
    {
        private MaskableGraphic _graph;
        private List<Tween> _tweenList;

        #region -- Unity APIs --
        protected void Awake()
        {
            _graph = GetComponent<MaskableGraphic>();
            _tweenList = new List<Tween>();
            OnAwake();
        }

        protected virtual void OnAwake()
        {

        }

        protected void OnDestroy()
        {
            Dispose(true);
            OnDestroyed();
        }

        protected virtual void OnDestroyed() { }
        #endregion

        #region -- Tween status --
        public bool IsAlive(Tween tween)
        {
            return tween != null && tween.IsActive();
        }

        public bool IsPause(Tween tween)
        {
            return IsAlive(tween) && !tween.IsPlaying();
        }

        public bool IsFinished(Tween tween)
        {
            return IsAlive(tween) && tween.IsComplete();
        }
        #endregion

        #region -- IDisposable --
        public void Dispose()
        {
            Dispose(false);
        }

        public virtual void Dispose(bool destroyed)
        {
            foreach (var tween in _tweenList)
            {
                if (IsAlive(tween))
                {
                    tween.Kill();
                }
            }
        }
        #endregion

        public virtual void Hide()
        {
            _graph.enabled = false;
        }

        public virtual void Display()
        {
            _graph.enabled = true;
        }

        public void PauseTweenAll()
        {
            foreach(var tween in _tweenList)
            {
                tween.Play();
            }
        }

        public void ResumeTweenAll()
        {
            foreach(var tween in _tweenList)
            {
                tween.Pause();
            }
        }

        public void Clear()
        {
            foreach(var tween in _tweenList)
            {
                if (IsAlive(tween))
                {
                    tween.Kill();
                }
            }
            _tweenList.Clear();
        }

        protected void AddTween(Tween tween)
        {
            if(!_tweenList.Contains(tween))
                _tweenList.Add(tween);
        }

        public Tween DoShake(float duration, float amptitude)
        {
            var tween = transform.DOShakePosition(duration, strength: amptitude);
            AddTween(tween);
            return tween;
        }

        public Tween DoColor(Color color, float duration)
        {
            if (_graph == null)
                return null;
            var tween = _graph.DOColor(color, duration).SetEase(Ease.Linear);
            return tween;
        }

        public Tween DoFade(float alpha, float duration)
        {
            if (_graph == null)
                return null;
            var tween = _graph.DOFade(alpha, duration).SetEase(Ease.Linear);
            return tween;
        }
    }
}
