using UnityEngine;
using DG.Tweening;

public class PlayerShootingEverHood : MonoBehaviour
{
    [SerializeField] private Transform topHalf;
    [SerializeField] private Transform rightArm;
    [SerializeField] private Transform leftArm;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private ArmRecoil leftArmRecoil;
    [SerializeField] private Transform leftArmFirePoint;
    [SerializeField] private ArmRecoil rightArmRecoil;
    [SerializeField] private Transform rightArmFirePoint;

    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float shootingCooldown = 0.15f;

    private bool isLeftArm = true;
    private float justShot = 0f;
    private Rigidbody rb;

    private PlayerSounds playerSounds;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerSounds = GetComponent<PlayerSounds>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        topHalf.DORotateQuaternion(Quaternion.LookRotation(transform.forward), 0.5f).OnComplete(() =>
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;

        });
        rightArm.DORotate(new Vector3(0f, 180f, 0f), 0.5f);
        leftArm.DORotate(new Vector3(0f, 180f, 0f), 0.5f);
    }

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        topHalf.DORotateQuaternion(Quaternion.LookRotation(transform.forward), 0.5f).OnComplete(() =>
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;

        });

        rightArm.DORotate(new Vector3(0f, 180f, 0f), 0.5f);
        leftArm.DORotate(new Vector3(0f, 180f, 0f), 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time - justShot >= shootingCooldown)
        {
            Shoot();
            justShot = Time.time;
        }
    }

    public void Shoot()
    {
        playerSounds.PlayerShoot();
        if (isLeftArm)
        {
            GameObject bullet = Instantiate(bulletPrefab, leftArmFirePoint.position, leftArmFirePoint.rotation);
            bullet.GetComponent<Rigidbody>().linearVelocity = bullet.transform.forward * bulletSpeed;
            leftArmRecoil.ApplyRecoil();
            isLeftArm = false;
        }
        else
        {
            GameObject bullet = Instantiate(bulletPrefab, rightArmFirePoint.position, rightArmFirePoint.rotation);
            bullet.GetComponent<Rigidbody>().linearVelocity = bullet.transform.forward * bulletSpeed;
            rightArmRecoil.ApplyRecoil();
            isLeftArm = true;
        }
    }
}