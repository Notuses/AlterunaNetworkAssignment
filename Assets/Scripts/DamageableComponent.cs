using Alteruna;
using System.Collections;
using UnityEngine;

public class DamageableComponent : AttributesSync
{
    private Alteruna.Avatar avatar;
    [SerializeField] Transform HealthBar;
    [SerializeField] Transform StunLocation;
    [SerializeField] ParticleSystem StunEmitter;
    private ParticleSystem StunEffect;

    [SerializeField] private float WorldDamageImmunityTime = 0.5f;
    [SerializeField] private int WorldDamage = 1;
    [SerializeField] private float MaxHealth = 10;
    [SynchronizableField] private float Health = 10;

    private PlayerMovement PlayerMovement;    
    private Vector3 stunVFXOffset;

    private Camera _cam;
    private CameraShaker _camShaker;
    private Spawner _spawner;
    
    private bool RecentlyDamaged = false;

    private PlayerStateSync platerState;


    private void Awake()
    {
        _cam = Camera.main;
        _camShaker = _cam.GetComponent<CameraShaker>();
        PlayerMovement = transform.parent.GetComponent<PlayerMovement>();
        platerState = transform.parent.GetComponentInChildren<PlayerStateSync>();
        avatar = gameObject.GetComponentInParent(typeof(Alteruna.Avatar)) as Alteruna.Avatar;
        _spawner = FindObjectOfType<Spawner>();
        
        stunVFXOffset = new Vector3(0, 2, 0);

    }

    private void Start()
    {
        StunEffect = Instantiate(StunEmitter, transform.position, StunLocation.transform.rotation);
        StunEffect.Stop();
        if (!avatar.IsMe)
            return;
        Health = MaxHealth;        
    }

    // Update is called once per frame
    void Update()
    {
        //Update size of healthbar
        Vector3 healthScale = new Vector3((Health / MaxHealth) * 2, HealthBar.localScale.y, HealthBar.localScale.z);
        HealthBar.localScale = healthScale;

        //turns healthbars to player camera.
        HealthBar.transform.LookAt(HealthBar.transform.position + _cam.transform.rotation * Vector3.forward);

        if (WorldManager.Instance.TakeWorldDamage(transform.parent.position) && !RecentlyDamaged)
            TakeWorldDamage();

                   
        StunEffect.transform.position = transform.position + stunVFXOffset;
              
            
    }
    
    private void TakeWorldDamage()
    {
        if (RecentlyDamaged)
            return;

        StartCoroutine(TickDamage());
    }

    private IEnumerator TickDamage()
    {
        RecentlyDamaged = true;

        yield return new WaitForSeconds(WorldDamageImmunityTime);

        RecentlyDamaged = false;

        if (!WorldManager.Instance.TakeWorldDamage(transform.parent.position))
        {
            yield break;
        }

        if (platerState.currentGameState == (byte)GameManager.State.StartRound)
            TakeDamage(WorldDamage);
    }
        
    
    public void OnHit(int damageAmount, Vector3 knockbackDirection, GameObject fromObject)
    {
        if (!avatar.IsMe) return;
        
        if (platerState.currentGameState == (byte)GameManager.State.StartRound)
            TakeDamage(damageAmount);
        
        PlayerMovement.SetAsStunned(0.5f);
        PlayerMovement.rb.AddForce(knockbackDirection * 150);
        _spawner.Despawn(fromObject);
    }
    void TakeDamage(int damageAmount)
    {
        if(Health > 0)
            Health -= damageAmount;

        BroadcastRemoteMethod("UpdateHealthBar");
        if (Health <= 0)
        {
            BroadcastRemoteMethod("Die");
        }

        if (WorldManager.Instance.TakeWorldDamage(transform.parent.position))
            return;

        if (avatar.IsMe)
            _camShaker.Shake(0.8f, 0.15f);
        BroadcastRemoteMethod("HitVFX");       
    }

    [SynchronizableMethod]
    void UpdateHealthBar()
    {
        if (!avatar.IsMe || Health < 0)
            return;
        
        Vector3 healthScale = new Vector3((Health / MaxHealth) * 2, HealthBar.localScale.y, HealthBar.localScale.z);
        HealthBar.localScale = healthScale;
    }

    [SynchronizableMethod]
    private void Die()
    {
        platerState.isAlive = false;
               
    }

    [SynchronizableMethod]
    private void HitVFX()
    {
        StunEffect.Play();
    }

    [SynchronizableMethod]
    public void ResetPlayerHealth()
    {
        platerState.isAlive = true;
        Health = MaxHealth;
    }

}
/*
 * According to all known laws
of aviation,

  
there is no way a bee
should be able to fly.

  
Its wings are too small to get
its fat little body off the ground.

  
The bee, of course, flies anyway

  
because bees don't care
what humans think is impossible.

  
Yellow, black. Yellow, black.
Yellow, black. Yellow, black.

  
Ooh, black and yellow!
Let's shake it up a little.

  
Barry! Breakfast is ready!

  
Ooming!

  
Hang on a second.

  
Hello?

  
- Barry?
- Adam?

  
- Oan you believe this is happening?
- I can't. I'll pick you up.

  
Looking sharp.

  
Use the stairs. Your father
paid good money for those.

  
Sorry. I'm excited.

  
Here's the graduate.
We're very proud of you, son.

  
A perfect report card, all B's.

  
Very proud.

  
Ma! I got a thing going here.

  
- You got lint on your fuzz.
- Ow! That's me!

  
- Wave to us! We'll be in row 118,000.
- Bye!

  
Barry, I told you,
stop flying in the house!

  
- Hey, Adam.
- Hey, Barry.

  
- Is that fuzz gel?
- A little. Special day, graduation.

  
Never thought I'd make it.

  
Three days grade school,
three days high school.

  
Those were awkward.

  
Three days college. I'm glad I took
a day and hitchhiked around the hive.

  
You did come back different.

  
- Hi, Barry.
- Artie, growing a mustache? Looks good.

  
- Hear about Frankie?
- Yeah.

  
- You going to the funeral?
- No, I'm not going.

  
Everybody knows,
sting someone, you die.

  
Don't waste it on a squirrel.
Such a hothead.

  
I guess he could have
just gotten out of the way.

  
I love this incorporating
an amusement park into our day.

  
That's why we don't need vacations.

  
Boy, quite a bit of pomp...
under the circumstances.

  
- Well, Adam, today we are men.
- We are!

  
- Bee-men.
- Amen!

  
Hallelujah!

  
Students, faculty, distinguished bees,

  
please welcome Dean Buzzwell.

  
Welcome, New Hive Oity
graduating class of...

  
...9:15.
*/
