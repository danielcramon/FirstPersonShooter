using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GunSystem : MonoBehaviour
{
    //Gun stats
    public int damage;
    public float timeBetweemShooting, spread, range, reloadTime, timeBetweenShots, shootForce;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    //bools
    bool shooting, readyToShoot, reloading;

    //Reference
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;
    public Transform playerTransform;
    public AudioSource reloadSound;
    public AudioSource fireSound;

    //Graphics
    public GameObject muzzleFlash, bullet;
    public CamShake camShake;
    public float camShakeMagnitube, camShakeDuration;
    public TextMeshProUGUI text;

    //Recoil
    public float currentRecoilYPos;
    public float currentRecoilXPos;
    [Range(0, 7f)]
    public float recoilAmountY;
    [Range(0, 3f)]
    public float recoilAmountX;
    private float timePressed;
    [Range(0, 10f)]
    public float maxRecoilTime;
    public MouseLook mls;

    private void Start()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;

        fpsCam = GameManager.instance.fpsCam;
        playerTransform = GameManager.instance.player.transform;
        text = GameManager.instance.bulletText;
        mls = GameManager.instance.mouseLookScript;
    }

    private void Update()
    {
        MyInput();
        if(text != null){
            text.SetText(bulletsLeft/bulletsPerTap + " / " + magazineSize/bulletsPerTap);
        }
    }

    private void MyInput()
    {
        if (allowButtonHold) 
        { 
            shooting = Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if(Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
        {
            Reload();
            reloadSound.Play();
        }

        if(readyToShoot && shooting && !reloading && bulletsLeft <= 0)
        {
            Reload();
            reloadSound.Play();
        }

        //shoot
        if(readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
            timePressed += Time.deltaTime;
            timePressed = timePressed >= maxRecoilTime ? maxRecoilTime : timePressed;
        }
        else
        {
            timePressed = 0;
        }
    }

    private void Reload()
    {
        reloading = true;

        if (!IsInvoking("ReloadFinished"))
        {
            Invoke("ReloadFinished", reloadTime);
        }
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    private void Shoot()
    {
        readyToShoot = false;

        //find the exact hit position using raycast
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        //Check if ray hits something
        Vector3 targetPoint;
        if(Physics.Raycast(ray, out rayHit))
        {
            targetPoint = rayHit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(75);
        }

        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        //Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        float z = Random.Range(-spread, spread);

        //Calculate Direction with Spread
        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, z);


        //ShakeCamera
        if(camShake != null)
        {
            StartCoroutine(camShake.Shake(camShakeDuration, camShakeMagnitube));
        }

        fireSound.Play();

        //Bullet
        GameObject currentBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);
        currentBullet.transform.forward = directionWithSpread.normalized;
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForce, ForceMode.Impulse);
        currentBullet.GetComponent<CustomBullets>().SetDamage(damage);

        //Graphics
        if(muzzleFlash != null)
        {
            GameObject currentFlash = Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
            currentFlash.GetComponent<ParticleSystem>().Play();
        }


        bulletsLeft--;
        bulletsShot--;

        if (!IsInvoking("ResetShot") && !readyToShoot)
        {
            //Add recoil to player
            RecoilMath();
            Invoke("ResetShot", timeBetweemShooting);
        }

        if(bulletsShot > 0 && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    public void RecoilMath()
    {
        currentRecoilXPos = ((Random.value - .5f) / 2) * recoilAmountX;
        currentRecoilYPos = ((Random.value - .5f) / 2) * (timePressed >= maxRecoilTime ? recoilAmountY / 4 : recoilAmountY);
        mls.wantedCameraXRotation -= Mathf.Abs(currentRecoilYPos);
        mls.wantedYRotation -= currentRecoilXPos;
    }
}
