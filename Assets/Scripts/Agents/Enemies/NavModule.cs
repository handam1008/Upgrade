using System.Threading.Tasks;
using DevLib.ModuleSystem;
using DevLib.ServiceLocator;
using DevLib.TileAstar;
using GameSystem.GameServices;
using UnityEngine;

namespace Agents.Enemies
{
    public class NavModule : MonoModule
    {
        [SerializeField] private int maxPathCorners = 16;
        [SerializeField] private float repathInterval = 0.3f; //경로 재계산 주기
        [SerializeField] private float cornerArriveDistance = 0.2f; //코너 도착 판정
        
        private IMapService _mapService;
        
        private PathAgent _pathAgent;
        private ITopDownMover _mover;

        private Vector3[] _path;
        private float _cornerArriveSqr;

        private Task<int> _pathTask;
        private int _pathLength;
        private int _currentIndex;
        private float _repathTimer;

        private bool _isActive;
        private Vector3 _destination;
        private Vector3 _requestedDestination; //이번 경로 계산에 실제로 사용한 목적지

        public bool IsActive => _isActive;
        public bool HasPath => _pathLength > 0;

        public override void Initialize(ModuleOwner owner)
        {
            base.Initialize(owner);
            _pathAgent = owner.GetComponent<PathAgent>();
            _mover = owner.GetModule<ITopDownMover>();
            
            _path = new Vector3[maxPathCorners];
            _cornerArriveSqr = cornerArriveDistance * cornerArriveDistance; //sqrMag로 비교할거라 제곱값 저장
        }

        private void Start()
        {
            _mapService = ServiceLocator.Get<IMapService>();
        }

        public void SetDestination(Vector3 destination)
        {
            _destination = destination;
            if (!_isActive)
            {
                _isActive = true;
                _repathTimer = 0f; //활성화 직후 바로 경로 계산한번 이뤄지도록 함.
            }
        }

        public void Stop()
        {
            if(_isActive)
                _mover.StopImmediately();
            
            _isActive = false;
            _pathAgent.CancelPath(); //캔슬시켜서 아까 PointArr를 덮어쓰는것을 방지시킨다.
            _pathTask = null;
            _pathLength = 0;
            _currentIndex = 0;
            _repathTimer = 0f;
        }

        private void Update()
        {
            if (!_isActive) return;

            //경로 계산이 완료되었다면 이걸 반영해주는 작업
            if (_pathTask is { IsCompleted: true })
            {
                if (_pathTask.Status == TaskStatus.RanToCompletion)
                {
                    int count = _pathTask.Result;
                    if (count > 0)
                    {
                        _pathLength = count;
                        //시작점 0은 어짜피 내가 시작하는 곳이니까 무시하고
                        _path[count - 1] = _requestedDestination;

                        //코너가 하나뿐인 경우면 시작셀이 목표셀이라 건너뛴다.
                        _currentIndex = count > 1 ? 1 : 0;
                    }
                }

                _pathTask = null;
            }
            
            //경로 계산이 완료되지 않았거나. 이미 완료되어서 반영까지 끝났지만 카운터가 돌아갔다면
            _repathTimer -= Time.deltaTime;
            if (_pathTask == null && (_repathTimer <= 0 || _pathLength == 0))
            {
                Vector3 selfPos = Owner.transform.position;
                _requestedDestination = _destination; //목표지점 설정.
                Vector3Int startCell = _mapService.GetWorldToCell(selfPos);
                Vector3Int destCell = _mapService.GetWorldToCell(_destination);
                _pathTask = _pathAgent.GetPath(startCell, destCell, _path);
                _repathTimer = repathInterval; //다음 경로 재계산을 위해 타이머 설정.
            }
            
            //그도 아니면 경로 계산도 되어있고, 반영도 되어있고, repathTimer도 아직 안지났다면
            //현재 경로를 따라서 움직여나가면 된다.
            FollowPath();
        }

        private void FollowPath()
        {
            if(_pathLength <= 0 || _currentIndex >= _pathLength) return;

            Vector3 selfPos = Owner.transform.position;
            Vector3 delta = _path[_currentIndex] - selfPos;
            delta.z = 0;

            if (delta.sqrMagnitude <= _cornerArriveSqr) //코너 도착했다면
            {
                _currentIndex++; 
                if(_currentIndex >= _pathLength)
                    _mover.StopImmediately();
            }
            else
            {
                _mover.SetMovement(delta);
            }
        }


    }
}