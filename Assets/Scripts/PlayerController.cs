using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _moveAcceleration = 30f;
    [SerializeField] private float _jumpSpeed = 8f;
    [SerializeField] private float _gravityAcceleration = 32f;
    [SerializeField] private float _groundedDistance = 0.2f;
    [SerializeField] private float _surfaceOffset = 0.2f;
    [SerializeField] private float _maxSurfaceDistance = 3.5f;
    [SerializeField] private float _orientationLerpSpeed = 12f;
    [SerializeField] private float _returnToSurfaceSpeed = 10f;

    private BoxCollider2D _currentPlatform;
    private Rigidbody2D _body;
    private Collider2D _collider;
    private float _moveInput;
    private bool _jumpQueued;
    private Vector2 _currentNormal = Vector2.up;
    private Vector2 _currentTangent = Vector2.right;    

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    public void SetMoveInput(float value) => _moveInput = Mathf.Clamp(value, -1f, 1f);

    public void QueueJump() => _jumpQueued = true;

    private void FixedUpdate()
    {
        ResolveNearestPlatformCollider();
        if (_currentPlatform == null) return;

        UpdateSurfaceFrame();

        var position = _body.position;
        var closestPoint = _currentPlatform.ClosestPoint(position);
        var surfaceDistance = Vector2.Distance(position, closestPoint);
        var grounded = surfaceDistance <= _surfaceOffset + _groundedDistance;
        var canJumpFromGround = grounded;
        var colliderGap = surfaceDistance;
        if (_collider != null)
        {
            var colliderDistance = _collider.Distance(_currentPlatform);
            colliderGap = colliderDistance.isOverlapped ? 0f : colliderDistance.distance;
            canJumpFromGround = canJumpFromGround || colliderGap <= _groundedDistance;
        }

        var velocity = _body.linearVelocity;
        var tangentVelocity = Vector2.Dot(velocity, _currentTangent);
        var normalVelocity = Vector2.Dot(velocity, _currentNormal);

        var targetTangentVelocity = _moveInput * _moveSpeed;
        tangentVelocity = Mathf.MoveTowards(tangentVelocity, targetTangentVelocity, _moveAcceleration * Time.fixedDeltaTime);

        normalVelocity -= _gravityAcceleration * Time.fixedDeltaTime;

        if (_jumpQueued && canJumpFromGround)
        {
            normalVelocity = _jumpSpeed;
            grounded = false;
        }

        _jumpQueued = false;

        if (grounded && normalVelocity < 0f)
        {
            normalVelocity = 0f;
        }

        velocity = _currentTangent * tangentVelocity + _currentNormal * normalVelocity;
        _body.linearVelocity = velocity;

        if (surfaceDistance > _maxSurfaceDistance)
        {
            var targetPosition = closestPoint + _currentNormal * _surfaceOffset;
            var newPosition = Vector2.MoveTowards(_body.position, targetPosition, _returnToSurfaceSpeed * Time.fixedDeltaTime);
            _body.MovePosition(newPosition);
        }
        else if (grounded)
        {
            var targetPosition = closestPoint + _currentNormal * _surfaceOffset;
            var newPosition = Vector2.MoveTowards(_body.position, targetPosition, _returnToSurfaceSpeed * Time.fixedDeltaTime);
            _body.MovePosition(newPosition);
        }
    }

    private void LateUpdate()
    {
        var targetRotation = Quaternion.FromToRotation(transform.up, _currentNormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _orientationLerpSpeed * Time.deltaTime);
    }

    private void UpdateSurfaceFrame()
    {
        var worldPosition = _body != null ? _body.position : (Vector2)transform.position;
        var closestPoint = _currentPlatform.ClosestPoint(worldPosition);
        var normal = worldPosition - closestPoint;

        if (normal.sqrMagnitude < 0.000001f)
        {
            Vector2 center = _currentPlatform.bounds.center;
            normal = worldPosition - center;
            if (normal.sqrMagnitude < 0.000001f)
            {
                normal = _currentNormal;
            }
        }

        _currentNormal = normal.normalized;

        var tangentA = new Vector2(-_currentNormal.y, _currentNormal.x).normalized;
        var tangentB = -tangentA;
        _currentTangent = Vector2.Dot(tangentA, _currentTangent) >= Vector2.Dot(tangentB, _currentTangent) ? tangentA : tangentB;
    }

    private void ResolveNearestPlatformCollider()
    {
        var colliders = FindObjectsByType<BoxCollider2D>(FindObjectsInactive.Exclude);
        if (colliders == null || colliders.Length == 0)
        {
            _currentPlatform = null;
            return;
        }

        var position = _body != null ? _body.position : (Vector2)transform.position;        
        var bestDistance = float.PositiveInfinity;

        BoxCollider2D nearest = null;
        for (int i = 0; i < colliders.Length; i++)
        {
            var candidate = colliders[i];
            if (candidate == null || candidate == _collider || candidate.isTrigger)
            {
                continue;
            }

            var candidateBody = candidate.attachedRigidbody;
            if (candidateBody != null && candidateBody.bodyType == RigidbodyType2D.Dynamic)
            {
                continue;
            }

            var closest = candidate.ClosestPoint(position);
            float distance = Vector2.SqrMagnitude(position - closest);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearest = candidate;
            }
        }

        _currentPlatform = nearest;
    }
}
