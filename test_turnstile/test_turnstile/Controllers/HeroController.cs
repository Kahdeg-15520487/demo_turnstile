using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using test_turnstile.Middleware;

namespace test_turnstile.Controllers
{
    //model: { id: 18, name: "Dr. IQ", power: "Really Smart", alterEgo: "Chuck Overstreet" }

    public class Hero
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Power { get; set; }
        public string AlterEgo { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class HeroController : ControllerBase
    {
        private static List<Hero> heroes = new List<Hero>
        {
        };

        // GET: api/<HeroController>
        [HttpGet]
        public IEnumerable<Hero> Get()
        {
            return heroes;
        }

        // GET api/<HeroController>/5
        [HttpGet("{id}")]
        public ActionResult<Hero> Get(int id)
        {
            var hero = heroes.FirstOrDefault(h => h.Id == id);
            if (hero == null)
            {
                return NotFound();
            }
            return hero;
        }

        // POST api/<HeroController>
        [HttpPost]
        [RequireTurnstileVerify]
        public ActionResult Post([FromBody] Hero hero)
        {
            heroes.Add(hero);
            return CreatedAtAction(nameof(Get), new { id = hero.Id }, hero);
        }

        // PUT api/<HeroController>/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Hero updatedHero)
        {
            var hero = heroes.FirstOrDefault(h => h.Id == id);
            if (hero == null)
            {
                return NotFound();
            }

            hero.Name = updatedHero.Name;
            hero.Power = updatedHero.Power;
            hero.AlterEgo = updatedHero.AlterEgo;

            return NoContent();
        }

        // DELETE api/<HeroController>/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var hero = heroes.FirstOrDefault(h => h.Id == id);
            if (hero == null)
            {
                return NotFound();
            }

            heroes.Remove(hero);
            return NoContent();
        }
    }
}
