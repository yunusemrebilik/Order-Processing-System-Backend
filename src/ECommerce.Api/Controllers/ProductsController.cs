using ECommerce.Application.Products.CreateProduct;
using ECommerce.Application.Products.DeleteProduct;
using ECommerce.Application.Products.GetProductById;
using ECommerce.Application.Products.GetProducts;
using ECommerce.Application.Products.UpdateProduct;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products with optional filtering, search, and pagination.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetProductsQuery
        {
            Category = category,
            Search = search,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a single product by its ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));

        if (result is null)
            return NotFound(new { message = $"Product with ID '{id}' not found" });

        return Ok(result);
    }

    /// <summary>
    /// Create a new product. Requires Admin role.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> CreateProduct(
        CreateProductCommand command,
        [FromServices] IValidator<CreateProductCommand> validator)
    {
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing product. Requires Admin role.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateProduct(
        string id,
        UpdateProductCommand command,
        [FromServices] IValidator<UpdateProductCommand> validator)
    {
        // Ensure the ID in the route matches the command
        var commandWithId = command with { Id = id };

        var validationResult = await validator.ValidateAsync(commandWithId);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        await _mediator.Send(commandWithId);
        return NoContent();
    }

    /// <summary>
    /// Soft-delete a product. Requires Admin role.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }
}
